using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using FinanzasPersonales.Api.Dtos;
using FinanzasPersonales.Api.Services;

namespace FinanzasPersonales.Api.Controllers
{
    [Route("api/gastos-compartidos")]
    [ApiController]
    [Produces("application/json")]
    [Authorize]
    public class GastosCompartidosController : ControllerBase
    {
        private readonly IGastosCompartidosService _service;

        public GastosCompartidosController(IGastosCompartidosService service)
        {
            _service = service;
        }

        [HttpGet]
        [ProducesResponseType(typeof(List<GastoCompartidoDto>), StatusCodes.Status200OK)]
        public async Task<ActionResult<List<GastoCompartidoDto>>> GetGastosCompartidos()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            return Ok(await _service.GetGastosCompartidosAsync(userId!));
        }

        [HttpGet("resumen")]
        [ProducesResponseType(typeof(ResumenSplitDto), StatusCodes.Status200OK)]
        public async Task<ActionResult<ResumenSplitDto>> GetResumen()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            return Ok(await _service.GetResumenAsync(userId!));
        }

        [HttpGet("{id}")]
        [ProducesResponseType(typeof(GastoCompartidoDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<GastoCompartidoDto>> GetGastoCompartido(int id)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var gasto = await _service.GetGastoCompartidoAsync(userId!, id);
            return gasto == null ? NotFound() : Ok(gasto);
        }

        [HttpPost]
        [ProducesResponseType(typeof(GastoCompartidoDto), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<GastoCompartidoDto>> CreateGastoCompartido(CreateGastoCompartidoDto dto)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            try
            {
                var gasto = await _service.CreateGastoCompartidoAsync(userId!, dto);
                return CreatedAtAction(nameof(GetGastoCompartido), new { id = gasto.Id }, gasto);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpDelete("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> DeleteGastoCompartido(int id)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var result = await _service.DeleteGastoCompartidoAsync(userId!, id);
            return result ? NoContent() : NotFound();
        }

        [HttpPut("{gastoId}/participantes/{participanteId}/liquidar")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> LiquidarParticipante(int gastoId, int participanteId, LiquidarParticipanteDto dto)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var result = await _service.LiquidarParticipanteAsync(userId!, gastoId, participanteId, dto);
            return result ? NoContent() : NotFound();
        }
    }
}
