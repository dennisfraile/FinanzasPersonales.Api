using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using FinanzasPersonales.Api.Dtos;
using FinanzasPersonales.Api.Services;

namespace FinanzasPersonales.Api.Controllers
{
    [Route("api/plantillas-ingreso")]
    [ApiController]
    [Produces("application/json")]
    [Authorize]
    public class PlantillasIngresoController : ControllerBase
    {
        private readonly IPlantillasIngresoService _plantillasService;

        public PlantillasIngresoController(IPlantillasIngresoService plantillasService)
        {
            _plantillasService = plantillasService;
        }

        [HttpGet]
        [ProducesResponseType(typeof(List<PlantillaIngresoDto>), StatusCodes.Status200OK)]
        public async Task<ActionResult<List<PlantillaIngresoDto>>> GetPlantillas()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            return Ok(await _plantillasService.GetPlantillasAsync(userId!));
        }

        [HttpPost]
        [ProducesResponseType(typeof(PlantillaIngresoDto), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<PlantillaIngresoDto>> CreatePlantilla(CreatePlantillaIngresoDto dto)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            try
            {
                var plantilla = await _plantillasService.CreatePlantillaAsync(userId!, dto);
                return CreatedAtAction(nameof(GetPlantillas), new { id = plantilla.Id }, plantilla);
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
        public async Task<IActionResult> UpdatePlantilla(int id, UpdatePlantillaIngresoDto dto)
        {
            if (id != dto.Id) return BadRequest("El ID de la URL no coincide con el del objeto.");
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            try
            {
                var result = await _plantillasService.UpdatePlantillaAsync(userId!, id, dto);
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
        public async Task<IActionResult> DeletePlantilla(int id)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var result = await _plantillasService.DeletePlantillaAsync(userId!, id);
            return result ? NoContent() : NotFound();
        }

        [HttpPost("{id}/usar")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<object>> UsarPlantilla(int id, UsarPlantillaIngresoDto dto)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            try
            {
                var ingreso = await _plantillasService.UsarPlantillaAsync(userId!, id, dto);
                return Ok(ingreso);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
}
