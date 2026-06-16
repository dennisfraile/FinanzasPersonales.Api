using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using FinanzasPersonales.Api.Dtos;
using FinanzasPersonales.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Google.Apis.Auth;

namespace FinanzasPersonales.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [EnableRateLimiting("auth")]
    public class AuthController : ControllerBase
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly IConfiguration _configuration;
        private readonly ILogger<AuthController> _logger;
        private readonly ITokenService _tokens;
        private readonly AuthCookieService _cookies;

        public AuthController(UserManager<IdentityUser> userManager, IConfiguration configuration, ILogger<AuthController> logger, ITokenService tokens, AuthCookieService cookies)
        {
            _userManager = userManager;
            _configuration = configuration;
            _logger = logger;
            _tokens = tokens;
            _cookies = cookies;
        }

        /// <summary>
        /// Autentica con Google. Recibe el ID token de Google, valida, crea/busca usuario y retorna JWT.
        /// </summary>
        [HttpPost("google")]
        [AllowAnonymous]
        [ProducesResponseType(typeof(AuthResponseDto), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(AuthResponseDto), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> GoogleLogin([FromBody] GoogleLoginDto dto)
        {
            if (string.IsNullOrEmpty(dto.IdToken))
            {
                return BadRequest(new AuthResponseDto { IsSuccess = false, Message = "Token de Google requerido." });
            }

            try
            {
                var settings = new GoogleJsonWebSignature.ValidationSettings
                {
                    Audience = new[] { _configuration["Google:ClientId"] }
                };

                var payload = await GoogleJsonWebSignature.ValidateAsync(dto.IdToken, settings);

                var user = await _userManager.FindByEmailAsync(payload.Email);

                if (user == null)
                {
                    user = new IdentityUser
                    {
                        Email = payload.Email,
                        UserName = payload.Name ?? payload.Email,
                        EmailConfirmed = true,
                        SecurityStamp = Guid.NewGuid().ToString()
                    };

                    var result = await _userManager.CreateAsync(user);
                    if (!result.Succeeded)
                    {
                        var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                        return BadRequest(new AuthResponseDto { IsSuccess = false, Message = $"Error al crear usuario: {errors}" });
                    }
                }

                var fotoUrl = payload.Picture;
                var accessJwt = _tokens.CreateAccessToken(user, fotoUrl);
                var refreshRaw = await _tokens.IssueRefreshTokenAsync(user.Id);
                var accessMin = int.TryParse(_configuration["Jwt:AccessTokenMinutes"], out var am) ? am : 15;
                var refreshDays = int.TryParse(_configuration["Jwt:RefreshTokenDays"], out var rd) ? rd : 7;
                _cookies.SetAuthCookies(Response, accessJwt, refreshRaw, accessMin, refreshDays);

                return Ok(new AuthResponseDto
                {
                    IsSuccess = true,
                    Message = "Inicio de sesion exitoso."
                });
            }
            catch (InvalidJwtException)
            {
                _logger.LogWarning("Failed Google login attempt with invalid token from IP {IP}", HttpContext.Connection.RemoteIpAddress);
                return BadRequest(new AuthResponseDto { IsSuccess = false, Message = "Token de Google invalido." });
            }
        }

        /// <summary>
        /// Obtiene el perfil del usuario autenticado
        /// </summary>
        [HttpGet("profile")]
        [Authorize]
        [ProducesResponseType(typeof(UserProfileDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetProfile()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
                return NotFound(new { Message = "Usuario no encontrado" });

            return Ok(new UserProfileDto
            {
                Id = user.Id,
                Email = user.Email ?? "",
                UserName = user.UserName,
                FotoUrl = User.FindFirstValue("FotoUrl")
            });
        }

        /// <summary>
        /// Actualiza el nombre de usuario
        /// </summary>
        [HttpPut("profile")]
        [Authorize]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> UpdateProfile([FromBody] UpdateUserProfileDto updateDto)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
                return NotFound(new { Message = "Usuario no encontrado" });

            if (!string.IsNullOrWhiteSpace(updateDto.UserName))
            {
                user.UserName = updateDto.UserName;
            }

            var result = await _userManager.UpdateAsync(user);
            if (!result.Succeeded)
            {
                var errors = result.Errors.Select(e => $"{e.Code}: {e.Description}").ToList();
                return BadRequest(new { Message = "Error al actualizar el perfil", Errors = errors });
            }

            return Ok(new { Message = "Perfil actualizado exitosamente" });
        }

        /// <summary>Rota el refresh token (cookie) y renueva el access token.</summary>
        [HttpPost("refresh")]
        [AllowAnonymous]
        public async Task<IActionResult> Refresh()
        {
            var raw = Request.Cookies[AuthCookieService.RefreshCookie];
            if (string.IsNullOrEmpty(raw))
                return Unauthorized(new AuthResponseDto { IsSuccess = false, Message = "Sin refresh token." });

            var rotated = await _tokens.RotateRefreshTokenAsync(raw);
            if (rotated == null)
            {
                _cookies.ClearAuthCookies(Response);
                return Unauthorized(new AuthResponseDto { IsSuccess = false, Message = "Refresh token invalido." });
            }

            var user = await _userManager.FindByIdAsync(rotated.Value.userId);
            if (user == null)
            {
                _cookies.ClearAuthCookies(Response);
                return Unauthorized(new AuthResponseDto { IsSuccess = false, Message = "Usuario no encontrado." });
            }

            var accessJwt = _tokens.CreateAccessToken(user, null);
            var accessMin = int.TryParse(_configuration["Jwt:AccessTokenMinutes"], out var am) ? am : 15;
            var refreshDays = int.TryParse(_configuration["Jwt:RefreshTokenDays"], out var rd) ? rd : 7;
            _cookies.SetAuthCookies(Response, accessJwt, rotated.Value.newRefreshRaw, accessMin, refreshDays);
            return Ok(new AuthResponseDto { IsSuccess = true, Message = "Token renovado." });
        }

        /// <summary>Revoca el refresh token y borra las cookies.</summary>
        [HttpPost("logout")]
        [AllowAnonymous]
        public async Task<IActionResult> Logout()
        {
            var raw = Request.Cookies[AuthCookieService.RefreshCookie];
            if (!string.IsNullOrEmpty(raw))
                await _tokens.RevokeAsync(raw);
            _cookies.ClearAuthCookies(Response);
            return Ok(new { Message = "Sesion cerrada." });
        }
    }
}
