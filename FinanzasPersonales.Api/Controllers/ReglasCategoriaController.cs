using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using FinanzasPersonales.Api.Dtos;
using FinanzasPersonales.Api.Services;

namespace FinanzasPersonales.Api.Controllers
{
    [Route("api/reglas-categoria")]
    [ApiController]
    [Produces("application/json")]
    [Authorize]
    public class ReglasCategoriaController : ControllerBase
    {
        private readonly IReglasCategoriaService _reglasService;

        public ReglasCategoriaController(IReglasCategoriaService reglasService)
        {
            _reglasService = reglasService;
        }

        [HttpGet]
        [ProducesResponseType(typeof(List<ReglaCategoriaDto>), StatusCodes.Status200OK)]
        public async Task<ActionResult<List<ReglaCategoriaDto>>> GetReglas()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var reglas = await _reglasService.GetReglasAsync(userId!);
            return Ok(reglas);
        }

        [HttpPost]
        [ProducesResponseType(typeof(ReglaCategoriaDto), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<ReglaCategoriaDto>> CreateRegla(CreateReglaCategoriaDto dto)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            try
            {
                var regla = await _reglasService.CreateReglaAsync(userId!, dto);
                return CreatedAtAction(nameof(GetReglas), new { id = regla.Id }, regla);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPut("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> UpdateRegla(int id, UpdateReglaCategoriaDto dto)
        {
            if (id != dto.Id) return BadRequest("El ID de la URL no coincide con el del objeto.");
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            try
            {
                var result = await _reglasService.UpdateReglaAsync(userId!, id, dto);
                return result ? NoContent() : NotFound();
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpDelete("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> DeleteRegla(int id)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var result = await _reglasService.DeleteReglaAsync(userId!, id);
            return result ? NoContent() : NotFound();
        }

        [HttpGet("sugerir")]
        [ProducesResponseType(typeof(CategoriaSugeridaDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        public async Task<IActionResult> SugerirCategoria([FromQuery] string descripcion, [FromQuery] string tipo = "Gasto")
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var sugerencia = await _reglasService.SugerirCategoriaAsync(userId!, descripcion, tipo);
            return sugerencia != null ? Ok(sugerencia) : NoContent();
        }
    }
}
