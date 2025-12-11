using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using FinanzasPersonales.Api.Data;
using FinanzasPersonales.Api.Dtos;
using FinanzasPersonales.Api.Models;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace FinanzasPersonales.Api.Controllers
{
    /// <summary>
    /// API para gestión de configuración del usuario.
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class ConfiguracionController : ControllerBase
    {
        private readonly FinanzasDbContext _context;

        public ConfiguracionController(FinanzasDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Obtiene la configuración del usuario
        /// </summary>
        [HttpGet]
        [ProducesResponseType(typeof(ConfiguracionUsuarioDto), StatusCodes.Status200OK)]
        public async Task<ActionResult<ConfiguracionUsuarioDto>> GetConfiguracion()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            var config = await _context.ConfiguracionesUsuario
                .FirstOrDefaultAsync(c => c.UserId == userId);

            if (config == null)
            {
                // Retornar configuración por defecto si no existe
                return Ok(new ConfiguracionUsuarioDto
                {
                    Moneda = "USD",
                    SimboloMoneda = "$",
                    Idioma = "es",
                    Tema = "light",
                    DiaInicioMes = 1,
                    MostrarSaldoInicial = true
                });
            }

            var dto = new ConfiguracionUsuarioDto
            {
                Moneda = config.Moneda,
                SimboloMoneda = config.SimboloMoneda,
                Idioma = config.Idioma,
                Tema = config.Tema,
                DiaInicioMes = config.DiaInicioMes,
                MostrarSaldoInicial = config.MostrarSaldoInicial
            };

            return Ok(dto);
        }

        /// <summary>
        /// Actualiza la configuración del usuario
        /// </summary>
        [HttpPut]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        public async Task<IActionResult> UpdateConfiguracion([FromBody] ConfiguracionUsuarioDto dto)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            var config = await _context.ConfiguracionesUsuario
                .FirstOrDefaultAsync(c => c.UserId == userId);

            if (config == null)
            {
                // Crear nueva configuración
                config = new ConfiguracionUsuario
                {
                    UserId = userId,
                    Moneda = dto.Moneda,
                    SimboloMoneda = dto.SimboloMoneda,
                    Idioma = dto.Idioma,
                    Tema = dto.Tema,
                    DiaInicioMes = dto.DiaInicioMes,
                    MostrarSaldoInicial = dto.MostrarSaldoInicial,
                    FechaCreacion = DateTime.Now
                };

                _context.ConfiguracionesUsuario.Add(config);
            }
            else
            {
                // Actualizar existente
                config.Moneda = dto.Moneda;
                config.SimboloMoneda = dto.SimboloMoneda;
                config.Idioma = dto.Idioma;
                config.Tema = dto.Tema;
                config.DiaInicioMes = dto.DiaInicioMes;
                config.MostrarSaldoInicial = dto.MostrarSaldoInicial;
                config.FechaActualizacion = DateTime.Now;
            }

            await _context.SaveChangesAsync();
            return NoContent();
        }

        /// <summary>
        /// Restablece la configuración a valores por defecto
        /// </summary>
        [HttpPost("restablecer")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        public async Task<IActionResult> RestablecerConfiguracion()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            var config = await _context.ConfiguracionesUsuario
                .FirstOrDefaultAsync(c => c.UserId == userId);

            if (config != null)
            {
                _context.ConfiguracionesUsuario.Remove(config);
                await _context.SaveChangesAsync();
            }

            return NoContent();
        }
    }
}
