using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using FinanzasPersonales.Api.Data;
using FinanzasPersonales.Api.Dtos;
using System.Security.Claims;

namespace FinanzasPersonales.Api.Controllers
{
    /// <summary>
    /// API para gestion del perfil de usuario.
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class PerfilController : ControllerBase
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly FinanzasDbContext _context;

        public PerfilController(UserManager<IdentityUser> userManager, FinanzasDbContext context)
        {
            _userManager = userManager;
            _context = context;
        }

        /// <summary>
        /// Obtiene la informacion del perfil del usuario autenticado.
        /// </summary>
        [HttpGet]
        [ProducesResponseType(typeof(PerfilUsuarioDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<PerfilUsuarioDto>> GetPerfil()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var user = await _userManager.FindByIdAsync(userId!);

            if (user == null)
                return NotFound("Usuario no encontrado.");

            var totalCategorias = await _context.Categorias.CountAsync(c => c.UserId == userId);
            var totalGastos = await _context.Gastos.CountAsync(g => g.UserId == userId);
            var totalIngresos = await _context.Ingresos.CountAsync(i => i.UserId == userId);
            var totalMetas = await _context.Metas.CountAsync(m => m.UserId == userId);

            var perfil = new PerfilUsuarioDto
            {
                Email = user.Email!,
                NombreCompleto = user.UserName,
                FotoUrl = User.FindFirstValue("FotoUrl"),
                FechaRegistro = user.LockoutEnd.HasValue ? user.LockoutEnd.Value.DateTime : DateTime.Now,
                TotalCategorias = totalCategorias,
                TotalGastos = totalGastos,
                TotalIngresos = totalIngresos,
                TotalMetas = totalMetas
            };

            return Ok(perfil);
        }

        /// <summary>
        /// Actualiza el nombre del usuario.
        /// </summary>
        [HttpPut]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> ActualizarPerfil(ActualizarPerfilDto dto)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var user = await _userManager.FindByIdAsync(userId!);

            if (user == null)
                return NotFound("Usuario no encontrado.");

            if (!string.IsNullOrWhiteSpace(dto.NombreCompleto))
            {
                user.UserName = dto.NombreCompleto;
            }

            var result = await _userManager.UpdateAsync(user);

            if (!result.Succeeded)
            {
                var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                return BadRequest($"Error al actualizar perfil: {errors}");
            }

            return Ok(new { Message = "Perfil actualizado exitosamente." });
        }
    }
}
