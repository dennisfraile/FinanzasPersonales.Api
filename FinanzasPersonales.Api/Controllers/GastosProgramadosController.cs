using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using FinanzasPersonales.Api.Dtos;
using System.Security.Claims;
using FinanzasPersonales.Api.Services;

namespace FinanzasPersonales.Api.Controllers
{
    /// <summary>
    /// API para gestión de gastos programados (recibos, cobros con fecha límite)
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class GastosProgramadosController : ControllerBase
    {
        private readonly IGastosProgramadosService _service;

        public GastosProgramadosController(IGastosProgramadosService service)
        {
            _service = service;
        }

        /// <summary>
        /// Obtiene todos los gastos programados del usuario, opcionalmente filtrados por estado
        /// </summary>
        [HttpGet]
        public async Task<ActionResult<IEnumerable<GastoProgramadoDto>>> GetGastosProgramados([FromQuery] string? estado = null)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            var resultado = await _service.GetGastosProgramadosAsync(userId, estado);
            return Ok(resultado);
        }

        /// <summary>
        /// Obtiene un gasto programado por ID
        /// </summary>
        [HttpGet("{id}")]
        public async Task<ActionResult<GastoProgramadoDto>> GetGastoProgramado(int id)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            var dto = await _service.GetGastoProgramadoAsync(userId, id);
            if (dto == null)
                return NotFound();

            return Ok(dto);
        }

        /// <summary>
        /// Crea un nuevo gasto programado (recibo, cobro con fecha límite)
        /// </summary>
        [HttpPost]
        public async Task<ActionResult<GastoProgramadoDto>> PostGastoProgramado(CreateGastoProgramadoDto dto)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            var (result, error) = await _service.CreateGastoProgramadoAsync(userId, dto);

            if (error != null)
                return BadRequest(new { error });

            return CreatedAtAction(nameof(GetGastoProgramado), new { id = result!.Id }, result);
        }

        /// <summary>
        /// Actualiza un gasto programado (ej: actualizar monto de recibo variable)
        /// </summary>
        [HttpPut("{id}")]
        public async Task<IActionResult> PutGastoProgramado(int id, UpdateGastoProgramadoDto dto)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            var (success, error) = await _service.UpdateGastoProgramadoAsync(userId, id, dto);

            if (error != null)
                return BadRequest(new { error });

            if (!success)
                return NotFound();

            return NoContent();
        }

        /// <summary>
        /// Elimina un gasto programado
        /// </summary>
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteGastoProgramado(int id)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            var success = await _service.DeleteGastoProgramadoAsync(userId, id);
            if (!success)
                return NotFound();

            return NoContent();
        }

        /// <summary>
        /// Registra el pago de un gasto programado.
        /// Crea el gasto real, descuenta de la cuenta y notifica al usuario.
        /// </summary>
        [HttpPost("{id}/pagar")]
        public async Task<ActionResult<GastoProgramadoDto>> PagarGastoProgramado(int id, PagarGastoProgramadoDto dto)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            var (result, error) = await _service.PagarGastoProgramadoAsync(userId, id, dto);

            if (error != null)
                return BadRequest(new { error });

            if (result == null)
                return NotFound();

            return Ok(result);
        }

        /// <summary>
        /// Cancela un gasto programado pendiente
        /// </summary>
        [HttpPost("{id}/cancelar")]
        public async Task<IActionResult> CancelarGastoProgramado(int id)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            var (success, error) = await _service.CancelarGastoProgramadoAsync(userId, id);

            if (error != null)
                return BadRequest(new { error });

            if (!success)
                return NotFound();

            return NoContent();
        }
    }
}
