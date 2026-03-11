using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using FinanzasPersonales.Api.Dtos;
using System.Security.Claims;
using FinanzasPersonales.Api.Services;

namespace FinanzasPersonales.Api.Controllers
{
    /// <summary>
    /// API para gestion de ingresos recurrentes
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class IngresosRecurrentesController : ControllerBase
    {
        private readonly IIngresosRecurrentesService _ingresosRecurrentesService;

        public IngresosRecurrentesController(IIngresosRecurrentesService ingresosRecurrentesService)
        {
            _ingresosRecurrentesService = ingresosRecurrentesService;
        }

        /// <summary>
        /// Obtiene todos los ingresos recurrentes del usuario
        /// </summary>
        [HttpGet]
        public async Task<ActionResult<IEnumerable<IngresoRecurrenteDto>>> GetIngresosRecurrentes()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            var recurrentes = await _ingresosRecurrentesService.GetIngresosRecurrentesAsync(userId);

            return Ok(recurrentes);
        }

        /// <summary>
        /// Obtiene un ingreso recurrente por ID
        /// </summary>
        [HttpGet("{id}")]
        public async Task<ActionResult<IngresoRecurrenteDto>> GetIngresoRecurrente(int id)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            var dto = await _ingresosRecurrentesService.GetIngresoRecurrenteAsync(userId, id);

            if (dto == null)
                return NotFound();

            return Ok(dto);
        }

        /// <summary>
        /// Crea un nuevo ingreso recurrente
        /// </summary>
        [HttpPost]
        public async Task<ActionResult<IngresoRecurrenteDto>> PostIngresoRecurrente(CreateIngresoRecurrenteDto dto)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            var (result, error) = await _ingresosRecurrentesService.CreateIngresoRecurrenteAsync(userId, dto);

            if (error != null)
                return BadRequest(error);

            return CreatedAtAction(nameof(GetIngresoRecurrente), new { id = result!.Id }, result);
        }

        /// <summary>
        /// Actualiza un ingreso recurrente
        /// </summary>
        [HttpPut("{id}")]
        public async Task<IActionResult> PutIngresoRecurrente(int id, UpdateIngresoRecurrenteDto dto)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            var (success, error) = await _ingresosRecurrentesService.UpdateIngresoRecurrenteAsync(userId, id, dto);

            if (error != null)
                return BadRequest(error);

            if (!success)
                return NotFound();

            return NoContent();
        }

        /// <summary>
        /// Elimina un ingreso recurrente
        /// </summary>
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteIngresoRecurrente(int id)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            var success = await _ingresosRecurrentesService.DeleteIngresoRecurrenteAsync(userId, id);

            if (!success)
                return NotFound();

            return NoContent();
        }

        /// <summary>
        /// Genera un ingreso ahora desde un recurrente especifico
        /// </summary>
        [HttpPost("{id}/generar")]
        public async Task<ActionResult> GenerarIngreso(int id)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            var (result, error) = await _ingresosRecurrentesService.GenerarIngresoAsync(userId, id);

            if (error != null)
                return BadRequest(error);

            if (result == null)
                return NotFound();

            return Ok(new { id = result.Id, mensaje = "Ingreso generado exitosamente" });
        }

        /// <summary>
        /// Genera todos los ingresos recurrentes pendientes
        /// </summary>
        [HttpPost("generar-pendientes")]
        public async Task<ActionResult<object>> GenerarPendientes()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            var generados = await _ingresosRecurrentesService.GenerarPendientesAsync(userId);

            return Ok(new { generados, mensaje = $"Se generaron {generados} ingresos recurrentes" });
        }
    }
}
