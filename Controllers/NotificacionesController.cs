using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using FinanzasPersonales.Api.Dtos;
using FinanzasPersonales.Api.Services;
using System.Security.Claims;

namespace FinanzasPersonales.Api.Controllers
{
    /// <summary>
    /// API para gestión de notificaciones del usuario.
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class NotificacionesController : ControllerBase
    {
        private readonly INotificacionService _notificacionService;

        public NotificacionesController(INotificacionService notificacionService)
        {
            _notificacionService = notificacionService;
        }

        /// <summary>
        /// Obtiene las notificaciones del usuario
        /// </summary>
        /// <param name="soloNoLeidas">Filtrar solo no leídas</param>
        [HttpGet]
        [ProducesResponseType(typeof(List<NotificacionDto>), StatusCodes.Status200OK)]
        public async Task<ActionResult<List<NotificacionDto>>> GetNotificaciones([FromQuery] bool soloNoLeidas = false)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            var notificaciones = await _notificacionService.ObtenerTodasAsync(userId, soloNoLeidas);
            return Ok(notificaciones);
        }

        /// <summary>
        /// Marca una notificación como leída
        /// </summary>
        [HttpPut("{id}/leer")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> MarcarComoLeida(int id)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            await _notificacionService.MarcarComoLeidaAsync(id, userId);
            return NoContent();
        }

        /// <summary>
        /// Obtiene la configuración de notificaciones del usuario (simulado)
        /// </summary>
        [HttpGet("configuracion")]
        [ProducesResponseType(typeof(ConfiguracionNotificacionesDto), StatusCodes.Status200OK)]
        public ActionResult<ConfiguracionNotificacionesDto> GetConfiguracion()
        {
            // Por ahora retornamos configuración por defecto
            // En una implementación completa, esto vendría de una tabla de configuración
            var config = new ConfiguracionNotificacionesDto
            {
                AlertasPresupuesto = true,
                UmbralPresupuesto = 80,
                AlertasMetas = true,
                DiasAntesMeta = 7,
                Email = User.FindFirstValue(ClaimTypes.Email)
            };

            return Ok(config);
        }

        /// <summary>
        /// Actualiza la configuración de notificaciones (simulado)
        /// </summary>
        [HttpPut("configuracion")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        public IActionResult UpdateConfiguracion([FromBody] ConfiguracionNotificacionesDto config)
        {
            // Por ahora solo retornamos NoContent
            // En una implementación completa, guardaríamos esto en una tabla
            return NoContent();
        }
    }
}
