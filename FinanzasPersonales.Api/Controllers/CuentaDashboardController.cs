using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using FinanzasPersonales.Api.Dtos;
using FinanzasPersonales.Api.Services;

namespace FinanzasPersonales.Api.Controllers
{
    /// <summary>
    /// Dashboard interactivo por cuenta individual con timeline de transacciones y surplus quincenal.
    /// </summary>
    [Route("api/dashboard/cuenta")]
    [ApiController]
    [Authorize]
    public class CuentaDashboardController : ControllerBase
    {
        private readonly ICuentaDashboardService _service;

        public CuentaDashboardController(ICuentaDashboardService service)
        {
            _service = service;
        }

        /// <summary>
        /// Obtiene el dashboard completo de una cuenta específica
        /// </summary>
        [HttpGet("{cuentaId}")]
        [ProducesResponseType(typeof(CuentaDashboardDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<CuentaDashboardDto>> GetCuentaDashboard(
            int cuentaId,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 50)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            var resultado = await _service.GetCuentaDashboardAsync(userId, cuentaId, page, pageSize);
            if (resultado == null)
                return NotFound(new { message = "Cuenta no encontrada" });

            return Ok(resultado);
        }

        /// <summary>
        /// Asigna el surplus de la quincena a BalanceInicial (ahorro) o a una Meta
        /// </summary>
        [HttpPost("asignar-surplus")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> AsignarSurplus([FromBody] AsignarSurplusDto dto)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            var (success, error) = await _service.AsignarSurplusAsync(userId, dto);

            if (!success)
                return BadRequest(new { message = error });

            return Ok(new { message = "Surplus asignado exitosamente" });
        }
    }
}
