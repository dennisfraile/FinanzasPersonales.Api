using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using FinanzasPersonales.Api.Dtos;
using FinanzasPersonales.Api.Services;
using System.Security.Claims;

namespace FinanzasPersonales.Api.Controllers
{
    [Route("api/reportes-programados")]
    [ApiController]
    [Produces("application/json")]
    [Authorize]
    public class ReportesProgramadosController : ControllerBase
    {
        private readonly IReportesProgramadosService _service;

        public ReportesProgramadosController(IReportesProgramadosService service)
        {
            _service = service;
        }

        private string GetUserId() => User.FindFirstValue(ClaimTypes.NameIdentifier)!;

        /// <summary>
        /// Obtiene todos los reportes programados del usuario
        /// </summary>
        [HttpGet]
        [ProducesResponseType(typeof(List<ReporteProgramadoDto>), StatusCodes.Status200OK)]
        public async Task<ActionResult<List<ReporteProgramadoDto>>> GetAll()
        {
            return Ok(await _service.GetAllAsync(GetUserId()));
        }

        /// <summary>
        /// Obtiene un reporte programado por ID
        /// </summary>
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(ReporteProgramadoDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<ReporteProgramadoDto>> GetById(int id)
        {
            var reporte = await _service.GetByIdAsync(GetUserId(), id);
            return reporte == null ? NotFound() : Ok(reporte);
        }

        /// <summary>
        /// Crea un nuevo reporte programado
        /// </summary>
        [HttpPost]
        [ProducesResponseType(typeof(ReporteProgramadoDto), StatusCodes.Status201Created)]
        public async Task<ActionResult<ReporteProgramadoDto>> Create(CreateReporteProgramadoDto dto)
        {
            var reporte = await _service.CreateAsync(GetUserId(), dto);
            return CreatedAtAction(nameof(GetById), new { id = reporte.Id }, reporte);
        }

        /// <summary>
        /// Actualiza un reporte programado existente
        /// </summary>
        [HttpPut("{id}")]
        [ProducesResponseType(typeof(ReporteProgramadoDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<ReporteProgramadoDto>> Update(int id, UpdateReporteProgramadoDto dto)
        {
            var reporte = await _service.UpdateAsync(GetUserId(), id, dto);
            return reporte == null ? NotFound() : Ok(reporte);
        }

        /// <summary>
        /// Elimina un reporte programado
        /// </summary>
        [HttpDelete("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Delete(int id)
        {
            var deleted = await _service.DeleteAsync(GetUserId(), id);
            return deleted ? NoContent() : NotFound();
        }
    }
}
