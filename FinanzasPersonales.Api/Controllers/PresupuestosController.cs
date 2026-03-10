using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using FinanzasPersonales.Api.Dtos;
using System.Security.Claims;
using FinanzasPersonales.Api.Services;

namespace FinanzasPersonales.Api.Controllers
{
    /// <summary>
    /// API para gestión de presupuestos por categoría.
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class PresupuestosController : ControllerBase
    {
        private readonly IPresupuestosService _presupuestosService;

        public PresupuestosController(IPresupuestosService presupuestosService)
        {
            _presupuestosService = presupuestosService;
        }

        /// <summary>
        /// Obtiene todos los presupuestos del usuario con información de progreso.
        /// </summary>
        [HttpGet]
        [ProducesResponseType(typeof(List<PresupuestoDto>), StatusCodes.Status200OK)]
        public async Task<ActionResult<List<PresupuestoDto>>> GetPresupuestos(
            [FromQuery] int? mes = null,
            [FromQuery] int? ano = null)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var resultado = await _presupuestosService.GetPresupuestosAsync(userId!, mes, ano);

            return Ok(resultado);
        }

        /// <summary>
        /// Obtiene un presupuesto específico por ID.
        /// </summary>
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(PresupuestoDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<PresupuestoDto>> GetPresupuesto(int id)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var resultado = await _presupuestosService.GetPresupuestoAsync(userId!, id);

            if (resultado == null)
                return NotFound();

            return Ok(resultado);
        }

        /// <summary>
        /// Crea un nuevo presupuesto.
        /// </summary>
        [HttpPost]
        [ProducesResponseType(typeof(PresupuestoDto), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<PresupuestoDto>> PostPresupuesto(PresupuestoCreateDto dto)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var (result, error) = await _presupuestosService.CreatePresupuestoAsync(userId!, dto);

            if (error != null)
                return BadRequest(error);

            return CreatedAtAction(nameof(GetPresupuesto), new { id = result!.Id }, result);
        }

        /// <summary>
        /// Actualiza un presupuesto existente.
        /// </summary>
        [HttpPut("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> PutPresupuesto(int id, PresupuestoUpdateDto dto)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var success = await _presupuestosService.UpdatePresupuestoAsync(userId!, id, dto);

            if (!success)
                return NotFound();

            return NoContent();
        }

        /// <summary>
        /// Elimina un presupuesto.
        /// </summary>
        [HttpDelete("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> DeletePresupuesto(int id)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var success = await _presupuestosService.DeletePresupuestoAsync(userId!, id);

            if (!success)
                return NotFound();

            return NoContent();
        }

        /// <summary>
        /// Obtiene presupuestos que están cerca del límite (>80%).
        /// </summary>
        [HttpGet("alertas")]
        [ProducesResponseType(typeof(List<PresupuestoDto>), StatusCodes.Status200OK)]
        public async Task<ActionResult<List<PresupuestoDto>>> GetAlertasPresupuesto()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var resultado = await _presupuestosService.GetAlertasAsync(userId!);

            return Ok(resultado);
        }
    }
}
