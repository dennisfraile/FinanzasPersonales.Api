using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using FinanzasPersonales.Api.Dtos;
using System.Security.Claims;
using FinanzasPersonales.Api.Services;

namespace FinanzasPersonales.Api.Controllers
{
    /// <summary>
    /// API para gestión de gastos recurrentes
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class GastosRecurrentesController : ControllerBase
    {
        private readonly IGastosRecurrentesService _gastosRecurrentesService;

        public GastosRecurrentesController(IGastosRecurrentesService gastosRecurrentesService)
        {
            _gastosRecurrentesService = gastosRecurrentesService;
        }

        /// <summary>
        /// Obtiene todos los gastos recurrentes del usuario
        /// </summary>
        [HttpGet]
        public async Task<ActionResult<IEnumerable<GastoRecurrenteDto>>> GetGastosRecurrentes()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            var recurrentes = await _gastosRecurrentesService.GetGastosRecurrentesAsync(userId);

            return Ok(recurrentes);
        }

        /// <summary>
        /// Obtiene un gasto recurrente por ID
        /// </summary>
        [HttpGet("{id}")]
        public async Task<ActionResult<GastoRecurrenteDto>> GetGastoRecurrente(int id)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            var dto = await _gastosRecurrentesService.GetGastoRecurrenteAsync(userId, id);

            if (dto == null)
                return NotFound();

            return Ok(dto);
        }

        /// <summary>
        /// Crea un nuevo gasto recurrente
        /// </summary>
        [HttpPost]
        public async Task<ActionResult<GastoRecurrenteDto>> PostGastoRecurrente(CreateGastoRecurrenteDto dto)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            var (result, error) = await _gastosRecurrentesService.CreateGastoRecurrenteAsync(userId, dto);

            if (error != null)
                return BadRequest(error);

            return CreatedAtAction(nameof(GetGastoRecurrente), new { id = result!.Id }, result);
        }

        /// <summary>
        /// Actualiza un gasto recurrente
        /// </summary>
        [HttpPut("{id}")]
        public async Task<IActionResult> PutGastoRecurrente(int id, UpdateGastoRecurrenteDto dto)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            var (success, error) = await _gastosRecurrentesService.UpdateGastoRecurrenteAsync(userId, id, dto);

            if (error != null)
                return BadRequest(error);

            if (!success)
                return NotFound();

            return NoContent();
        }

        /// <summary>
        /// Elimina un gasto recurrente
        /// </summary>
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteGastoRecurrente(int id)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            var success = await _gastosRecurrentesService.DeleteGastoRecurrenteAsync(userId, id);

            if (!success)
                return NotFound();

            return NoContent();
        }

        /// <summary>
        /// Genera un gasto ahora desde un recurrente específico
        /// </summary>
        [HttpPost("{id}/generar")]
        public async Task<ActionResult<Models.Gasto>> GenerarGasto(int id)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            var (result, error) = await _gastosRecurrentesService.GenerarGastoAsync(userId, id);

            if (error != null)
                return BadRequest(error);

            if (result == null)
                return NotFound();

            return CreatedAtAction("GetGasto", "Gastos", new { id = result.Id }, result);
        }

        /// <summary>
        /// Genera todos los gastos recurrentes pendientes (para cron job)
        /// </summary>
        [HttpPost("generar-pendientes")]
        public async Task<ActionResult<object>> GenerarPendientes()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            var generados = await _gastosRecurrentesService.GenerarPendientesAsync(userId);

            return Ok(new { generados, mensaje = $"Se generaron {generados} gastos recurrentes" });
        }
    }
}
