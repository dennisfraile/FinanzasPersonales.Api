using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using FinanzasPersonales.Api.Dtos;
using FinanzasPersonales.Api.Services;

namespace FinanzasPersonales.Api.Controllers
{
    [Route("api/deudas")]
    [ApiController]
    [Produces("application/json")]
    [Authorize]
    public class DeudasController : ControllerBase
    {
        private readonly IDeudasService _deudasService;

        public DeudasController(IDeudasService deudasService)
        {
            _deudasService = deudasService;
        }

        [HttpGet]
        [ProducesResponseType(typeof(List<DeudaDto>), StatusCodes.Status200OK)]
        public async Task<ActionResult<List<DeudaDto>>> GetDeudas()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            return Ok(await _deudasService.GetDeudasAsync(userId!));
        }

        [HttpGet("{id}")]
        [ProducesResponseType(typeof(DeudaDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<DeudaDto>> GetDeuda(int id)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var deuda = await _deudasService.GetDeudaAsync(userId!, id);
            return deuda == null ? NotFound() : Ok(deuda);
        }

        [HttpPost]
        [ProducesResponseType(typeof(DeudaDto), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<DeudaDto>> CreateDeuda(CreateDeudaDto dto)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            try
            {
                var deuda = await _deudasService.CreateDeudaAsync(userId!, dto);
                return CreatedAtAction(nameof(GetDeuda), new { id = deuda.Id }, deuda);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPut("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> UpdateDeuda(int id, UpdateDeudaDto dto)
        {
            if (id != dto.Id) return BadRequest("El ID de la URL no coincide con el del objeto.");
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var result = await _deudasService.UpdateDeudaAsync(userId!, id, dto);
            return result ? NoContent() : NotFound();
        }

        [HttpDelete("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> DeleteDeuda(int id)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var result = await _deudasService.DeleteDeudaAsync(userId!, id);
            return result ? NoContent() : NotFound();
        }

        [HttpPost("{id}/pagos")]
        [ProducesResponseType(typeof(PagoDeudaDto), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<PagoDeudaDto>> RegistrarPago(int id, CreatePagoDeudaDto dto)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            try
            {
                var pago = await _deudasService.RegistrarPagoAsync(userId!, id, dto);
                return CreatedAtAction(nameof(GetPagos), new { id }, pago);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpGet("{id}/pagos")]
        [ProducesResponseType(typeof(List<PagoDeudaDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<List<PagoDeudaDto>>> GetPagos(int id)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            try
            {
                var pagos = await _deudasService.GetPagosAsync(userId!, id);
                return Ok(pagos);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpGet("{id}/proyeccion")]
        [ProducesResponseType(typeof(List<ProyeccionPagoDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<List<ProyeccionPagoDto>>> GetProyeccion(int id, [FromQuery] decimal? pagoMensual = null)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            try
            {
                var proyeccion = await _deudasService.GetProyeccionAsync(userId!, id, pagoMensual);
                return Ok(proyeccion);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
}
