using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using FinanzasPersonales.Api.Dtos; // Nuestros DTOs
using Microsoft.AspNetCore.Authorization; // Para [AllowAnonymous]

namespace FinanzasPersonales.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly IConfiguration _configuration;

        // Inyectamos los servicios de Identity y Configuración
        public AuthController(UserManager<IdentityUser> userManager, IConfiguration configuration)
        {
            _userManager = userManager;
            _configuration = configuration;
        }

        /// <summary>
        /// Registra un nuevo usuario en el sistema.
        /// </summary>
        [HttpPost("register")]
        [AllowAnonymous] // Permite el acceso a este endpoint sin un token
        [ProducesResponseType(typeof(AuthResponseDto), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(AuthResponseDto), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Register([FromBody] RegisterDto registerDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new AuthResponseDto { IsSuccess = false, Message = "Datos de registro inválidos." });
            }

            var userExists = await _userManager.FindByEmailAsync(registerDto.Email);
            if (userExists != null)
            {
                return BadRequest(new AuthResponseDto { IsSuccess = false, Message = "El correo electrónico ya está en uso." });
            }

            IdentityUser user = new()
            {
                Email = registerDto.Email,
                UserName = registerDto.Email, // Usamos el email como UserName
                SecurityStamp = Guid.NewGuid().ToString()
            };

            var result = await _userManager.CreateAsync(user, registerDto.Password);

            if (!result.Succeeded)
            {
                var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                return BadRequest(new AuthResponseDto { IsSuccess = false, Message = $"Error al crear usuario: {errors}" });
            }

            // Opcional: Asignar un rol por defecto, ej. "Usuario"
            // await _userManager.AddToRoleAsync(user, "Usuario");

            return Ok(new AuthResponseDto { IsSuccess = true, Message = "¡Usuario creado exitosamente!" });
        }

        /// <summary>
        /// Inicia sesión (autentica) un usuario y devuelve un token JWT.
        /// </summary>
        [HttpPost("login")]
        [AllowAnonymous]
        [ProducesResponseType(typeof(AuthResponseDto), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(AuthResponseDto), StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> Login([FromBody] LoginDto loginDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new AuthResponseDto { IsSuccess = false, Message = "Datos de inicio de sesión inválidos." });
            }

            var user = await _userManager.FindByEmailAsync(loginDto.Email);

            // Verificamos al usuario Y su contraseña
            if (user != null && await _userManager.CheckPasswordAsync(user, loginDto.Password))
            {
                // --- Generación del Token JWT ---
                var tokenString = GenerateJwtToken(user);

                return Ok(new AuthResponseDto
                {
                    IsSuccess = true,
                    Message = "Inicio de sesión exitoso.",
                    Token = tokenString
                });
            }

            // Si el usuario no existe o la contraseña es incorrecta
            return Unauthorized(new AuthResponseDto { IsSuccess = false, Message = "Correo o contraseña inválidos." });
        }


        // --- MÉTODO PRIVADO PARA GENERAR EL TOKEN ---
        private string GenerateJwtToken(IdentityUser user)
        {
            var jwtKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]));
            var credentials = new SigningCredentials(jwtKey, SecurityAlgorithms.HmacSha256);

            // Los "Claims" son la información que guardamos dentro del token
            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id), // ID del usuario (ÚNICO claim de identidad)
                new Claim(JwtRegisteredClaimNames.Email, user.Email),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()) // ID único del token
            };

            var token = new JwtSecurityToken(
                issuer: _configuration["Jwt:Issuer"],
                audience: _configuration["Jwt:Audience"],
                claims: claims,
                expires: DateTime.Now.AddHours(24), // El token expira en 24 horas
                signingCredentials: credentials);

            return new JwtSecurityTokenHandler().WriteToken(token);
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
                UserName = user.UserName
            });
        }

        /// <summary>
        /// Actualiza el perfil del usuario
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

        /// <summary>
        /// Cambia la contraseña del usuario
        /// </summary>
        [HttpPut("change-password")]
        [Authorize]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordDto changePasswordDto)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
                return NotFound(new { Message = "Usuario no encontrado" });

            var result = await _userManager.ChangePasswordAsync(user, changePasswordDto.CurrentPassword, changePasswordDto.NewPassword);
            if (!result.Succeeded)
            {
                return BadRequest(new { Message = "Error al cambiar la contraseña", Errors = result.Errors });
            }

            return Ok(new { Message = "Contraseña cambiada exitosamente" });
        }
    }
}
