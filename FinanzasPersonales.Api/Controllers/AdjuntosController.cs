using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using FinanzasPersonales.Api.Data;
using FinanzasPersonales.Api.Models;
using FinanzasPersonales.Api.Services;
using System.Security.Claims;

namespace FinanzasPersonales.Api.Controllers
{
    /// <summary>
    /// API para gestión de adjuntos (comprobantes, facturas)
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class AdjuntosController : ControllerBase
    {
        private readonly FinanzasDbContext _context;
        private readonly IFileStorageService _fileStorage;
        private readonly ILogger<AdjuntosController> _logger;
        private const long MaxFileSize = 5 * 1024 * 1024; // 5MB
        private static readonly string[] AllowedExtensions = { ".pdf" };

        public AdjuntosController(
            FinanzasDbContext context,
            IFileStorageService fileStorage,
            ILogger<AdjuntosController> logger)
        {
            _context = context;
            _fileStorage = fileStorage;
            _logger = logger;
        }

        /// <summary>
        /// Sube un adjunto para un gasto o ingreso
        /// </summary>
        [HttpPost("upload")]
        [Consumes("multipart/form-data")]
        public async Task<ActionResult<Adjunto>> UploadFile(
            [FromForm] IFormFile file,
            [FromForm] int? gastoId = null,
            [FromForm] int? ingresoId = null)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            // Validaciones
            if (file == null || file.Length == 0)
                return BadRequest("No se proporcionó ningún archivo");

            if (file.Length > MaxFileSize)
                return BadRequest($"El archivo excede el tamaño máximo permitido de 5MB");

            var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
            if (!AllowedExtensions.Contains(extension))
                return BadRequest("Solo se permiten archivos PDF. Por favor, escanea tus comprobantes como PDF.");

            if (!gastoId.HasValue && !ingresoId.HasValue)
                return BadRequest("Debe especificar un gastoId o ingresoId");

            if (gastoId.HasValue && ingresoId.HasValue)
                return BadRequest("No puede especificar ambos gastoId e ingresoId");

            // Verificar que el gasto/ingreso existe y pertenece al usuario
            if (gastoId.HasValue)
            {
                var gasto = await _context.Gastos.FindAsync(gastoId.Value);
                if (gasto == null || gasto.UserId != userId)
                    return NotFound("Gasto no encontrado");
            }

            if (ingresoId.HasValue)
            {
                var ingreso = await _context.Ingresos.FindAsync(ingresoId.Value);
                if (ingreso == null || ingreso.UserId != userId)
                    return NotFound("Ingreso no encontrado");
            }

            try
            {
                // Guardar archivo
                var filePath = await _fileStorage.SaveFileAsync(file, userId);

                // Crear registro en BD
                var adjunto = new Adjunto
                {
                    FileName = file.FileName,
                    FilePath = filePath,
                    ContentType = file.ContentType,
                    FileSize = file.Length,
                    GastoId = gastoId,
                    IngresoId = ingresoId,
                    UserId = userId
                };

                _context.Adjuntos.Add(adjunto);
                await _context.SaveChangesAsync();

                _logger.LogInformation("Adjunto {Id} subido para user {UserId}", adjunto.Id, userId);

                return CreatedAtAction(nameof(GetAdjunto), new { id = adjunto.Id }, adjunto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al subir adjunto para user {UserId}", userId);
                return StatusCode(500, "Error al guardar el archivo");
            }
        }

        /// <summary>
        /// Obtiene información de un adjunto
        /// </summary>
        [HttpGet("{id}")]
        public async Task<ActionResult<Adjunto>> GetAdjunto(int id)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            var adjunto = await _context.Adjuntos.FindAsync(id);

            if (adjunto == null || adjunto.UserId != userId)
                return NotFound();

            return Ok(adjunto);
        }

        /// <summary>
        /// Descarga un adjunto
        /// </summary>
        [HttpGet("{id}/download")]
        public async Task<IActionResult> DownloadFile(int id)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            var adjunto = await _context.Adjuntos.FindAsync(id);

            if (adjunto == null || adjunto.UserId != userId)
                return NotFound();

            try
            {
                var fileBytes = await _fileStorage.GetFileAsync(adjunto.FilePath);
                return File(fileBytes, adjunto.ContentType, adjunto.FileName);
            }
            catch (FileNotFoundException)
            {
                _logger.LogWarning("Archivo no encontrado: {Path}", adjunto.FilePath);
                return NotFound("Archivo no encontrado en el servidor");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al descargar adjunto {Id}", id);
                return StatusCode(500, "Error al descargar el archivo");
            }
        }

        /// <summary>
        /// Lista adjuntos de un gasto
        /// </summary>
        [HttpGet("gasto/{gastoId}")]
        public async Task<ActionResult<IEnumerable<Adjunto>>> GetAdjuntosByGasto(int gastoId)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            // Verificar que el gasto existe y pertenece al usuario
            var gasto = await _context.Gastos.FindAsync(gastoId);
            if (gasto == null || gasto.UserId != userId)
                return NotFound();

            var adjuntos = await _context.Adjuntos
                .Where(a => a.GastoId == gastoId)
                .ToListAsync();

            return Ok(adjuntos);
        }

        /// <summary>
        /// Lista adjuntos de un ingreso
        /// </summary>
        [HttpGet("ingreso/{ingresoId}")]
        public async Task<ActionResult<IEnumerable<Adjunto>>> GetAdjuntosByIngreso(int ingresoId)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            // Verificar que el ingreso existe y pertenece al usuario
            var ingreso = await _context.Ingresos.FindAsync(ingresoId);
            if (ingreso == null || ingreso.UserId != userId)
                return NotFound();

            var adjuntos = await _context.Adjuntos
                .Where(a => a.IngresoId == ingresoId)
                .ToListAsync();

            return Ok(adjuntos);
        }

        /// <summary>
        /// Elimina un adjunto
        /// </summary>
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteAdjunto(int id)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            var adjunto = await _context.Adjuntos.FindAsync(id);

            if (adjunto == null || adjunto.UserId != userId)
                return NotFound();

            try
            {
                // Eliminar archivo del storage
                await _fileStorage.DeleteFileAsync(adjunto.FilePath);

                // Eliminar registro de BD
                _context.Adjuntos.Remove(adjunto);
                await _context.SaveChangesAsync();

                _logger.LogInformation("Adjunto {Id} eliminado por user {UserId}", id, userId);

                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al eliminar adjunto {Id}", id);
                return StatusCode(500, "Error al eliminar el archivo");
            }
        }
    }
}
