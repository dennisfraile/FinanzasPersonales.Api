using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using System.Security.Claims;
using System.Text.Json;
using FinanzasPersonales.Api.Dtos;
using FinanzasPersonales.Api.Services;

namespace FinanzasPersonales.Api.Controllers
{
    [Route("api/importacion")]
    [ApiController]
    [Produces("application/json")]
    [Authorize]
    [EnableRateLimiting("uploads")]
    public class ImportacionController : ControllerBase
    {
        private readonly IImportacionCsvService _importService;
        private const long MaxCsvFileSize = 5 * 1024 * 1024; // 5MB

        public ImportacionController(IImportacionCsvService importService)
        {
            _importService = importService;
        }

        /// <summary>
        /// Sube un CSV y devuelve las primeras filas con nombres de columnas para configurar el mapeo.
        /// </summary>
        [HttpPost("preview")]
        [ProducesResponseType(typeof(CsvPreviewResponseDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<CsvPreviewResponseDto>> Preview(IFormFile archivo)
        {
            if (archivo == null || archivo.Length == 0)
                return BadRequest("Debe subir un archivo CSV.");

            if (archivo.Length > MaxCsvFileSize)
                return BadRequest("El archivo CSV excede el tamaño máximo de 5MB.");

            var extension = Path.GetExtension(archivo.FileName).ToLowerInvariant();
            if (extension != ".csv")
                return BadRequest("Solo se permiten archivos CSV.");

            using var stream = archivo.OpenReadStream();
            var result = await _importService.PreviewCsvAsync(stream);
            return Ok(result);
        }

        /// <summary>
        /// Sube un CSV con mapeo de columnas y devuelve un preview validado con duplicados y categorías sugeridas.
        /// </summary>
        [HttpPost("validate")]
        [ProducesResponseType(typeof(List<CsvPreviewRowDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<List<CsvPreviewRowDto>>> Validate(IFormFile archivo, [FromForm] string request)
        {
            if (archivo == null || archivo.Length == 0)
                return BadRequest("Debe subir un archivo CSV.");

            if (archivo.Length > MaxCsvFileSize)
                return BadRequest("El archivo CSV excede el tamaño máximo de 5MB.");

            var importRequest = JsonSerializer.Deserialize<CsvImportRequestDto>(request, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            if (importRequest == null)
                return BadRequest("Datos de importación inválidos.");

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            using var stream = archivo.OpenReadStream();
            var result = await _importService.ValidateAndPreviewAsync(userId!, stream, importRequest);
            return Ok(result);
        }

        /// <summary>
        /// Ejecuta la importación del CSV creando gastos e ingresos.
        /// </summary>
        [HttpPost("ejecutar")]
        [ProducesResponseType(typeof(CsvImportResultDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<CsvImportResultDto>> Ejecutar(IFormFile archivo, [FromForm] string request)
        {
            if (archivo == null || archivo.Length == 0)
                return BadRequest("Debe subir un archivo CSV.");

            if (archivo.Length > MaxCsvFileSize)
                return BadRequest("El archivo CSV excede el tamaño máximo de 5MB.");

            var importRequest = JsonSerializer.Deserialize<CsvImportRequestDto>(request, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            if (importRequest == null)
                return BadRequest("Datos de importación inválidos.");

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            try
            {
                using var stream = archivo.OpenReadStream();
                var result = await _importService.ImportCsvAsync(userId!, stream, importRequest, archivo.FileName);
                return Ok(result);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
}
