using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using FinanzasPersonales.Api.Dtos;
using FinanzasPersonales.Api.Services;
using System.Security.Claims;

namespace FinanzasPersonales.Api.Controllers
{
    /// <summary>
    /// API para exportación de datos financieros en diferentes formatos.
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    [Produces("application/json")]
    public class ExportController : ControllerBase
    {
        private readonly IExportService _exportService;

        public ExportController(IExportService exportService)
        {
            _exportService = exportService;
        }

        /// <summary>
        /// Exporta datos a formato Excel (.xlsx)
        /// </summary>
        /// <param name="request">Parámetros de exportación</param>
        /// <returns>Archivo Excel con los datos solicitados</returns>
        [HttpPost("excel")]
        [ProducesResponseType(typeof(FileResult), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> ExportToExcel([FromBody] ExportRequestDto request)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            // Valores por defecto si no se especifican fechas
            var desde = request.Desde ?? new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);
            var hasta = request.Hasta ?? DateTime.Now;

            // Valores por defecto para incluir (todo si no se especifica)
            var incluir = request.Incluir ?? new List<string> { "gastos", "ingresos", "metas", "presupuestos" };

            var excelBytes = await _exportService.ExportToExcelAsync(userId, desde, hasta, incluir);

            var fileName = $"FinanzasPersonales_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx";
            return File(excelBytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
        }

        /// <summary>
        /// Exporta datos a formato PDF
        /// </summary>
        /// <param name="request">Parámetros de exportación</param>
        /// <returns>Archivo PDF con los datos solicitados</returns>
        [HttpPost("pdf")]
        [ProducesResponseType(typeof(FileResult), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> ExportToPdf([FromBody] ExportRequestDto request)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            var desde = request.Desde ?? new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);
            var hasta = request.Hasta ?? DateTime.Now;
            var incluir = request.Incluir ?? new List<string> { "gastos", "ingresos" };

            var pdfBytes = await _exportService.ExportToPdfAsync(userId, desde, hasta, incluir);

            var fileName = $"ReporteFinanciero_{DateTime.Now:yyyyMMdd_HHmmss}.pdf";
            return File(pdfBytes, "application/pdf", fileName);
        }

        /// <summary>
        /// Crea backup completo de datos en formato JSON
        /// </summary>
        /// <param name="desde">Fecha inicial opcional para filtrar datos</param>
        /// <param name="hasta">Fecha final opcional para filtrar datos</param>
        /// <returns>Archivo JSON con backup de datos</returns>
        [HttpPost("backup")]
        [ProducesResponseType(typeof(FileResult), StatusCodes.Status200OK)]
        public async Task<IActionResult> ExportBackup([FromQuery] DateTime? desde = null, [FromQuery] DateTime? hasta = null)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            var jsonContent = await _exportService.ExportToJsonAsync(userId, desde, hasta);

            var fileName = $"Backup_FinanzasPersonales_{DateTime.Now:yyyyMMdd_HHmmss}.json";
            var bytes = System.Text.Encoding.UTF8.GetBytes(jsonContent);

            return File(bytes, "application/json", fileName);
        }
    }
}
