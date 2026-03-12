using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using FinanzasPersonales.Api.Dtos;
using FinanzasPersonales.Api.Services;

namespace FinanzasPersonales.Api.Controllers
{
    [Route("api/tipo-cambio")]
    [ApiController]
    [Produces("application/json")]
    [Authorize]
    public class TipoCambioController : ControllerBase
    {
        private readonly ITipoCambioService _tipoCambioService;

        public TipoCambioController(ITipoCambioService tipoCambioService)
        {
            _tipoCambioService = tipoCambioService;
        }

        [HttpGet]
        [ProducesResponseType(typeof(TipoCambioDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetTasaActual([FromQuery] string origen, [FromQuery] string destino)
        {
            var tasa = await _tipoCambioService.GetTasaActualAsync(origen.ToUpper(), destino.ToUpper());
            return tasa != null ? Ok(tasa) : NotFound($"No existe tasa de cambio de {origen} a {destino}.");
        }

        [HttpPost]
        [ProducesResponseType(typeof(TipoCambioDto), StatusCodes.Status201Created)]
        public async Task<ActionResult<TipoCambioDto>> CreateTipoCambio(CreateTipoCambioDto dto)
        {
            var tipo = await _tipoCambioService.CreateTipoCambioAsync(dto);
            return CreatedAtAction(nameof(GetTasaActual), new { origen = tipo.MonedaOrigen, destino = tipo.MonedaDestino }, tipo);
        }

        [HttpGet("convertir")]
        [ProducesResponseType(typeof(ConversionDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<ConversionDto>> Convertir([FromQuery] decimal monto, [FromQuery] string origen, [FromQuery] string destino)
        {
            try
            {
                var resultado = await _tipoCambioService.ConvertirAsync(monto, origen.ToUpper(), destino.ToUpper());
                return Ok(resultado);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpGet("historial")]
        [ProducesResponseType(typeof(List<TipoCambioDto>), StatusCodes.Status200OK)]
        public async Task<ActionResult<List<TipoCambioDto>>> GetHistorial([FromQuery] string origen, [FromQuery] string destino, [FromQuery] int limite = 30)
        {
            return Ok(await _tipoCambioService.GetHistorialAsync(origen.ToUpper(), destino.ToUpper(), limite));
        }
    }
}
