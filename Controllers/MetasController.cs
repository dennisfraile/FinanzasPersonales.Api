using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using FinanzasPersonales.Api.Data;
using FinanzasPersonales.Api.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Authorization; // Para [Authorize]
using System.Security.Claims; // Para obtener el ID del usuario desde el token

namespace FinanzasPersonales.Api.Controllers
{
    /// <summary>
    /// API para gestionar todos los gastos de la aplicación.
    /// </summary>
    [Route("api/[controller]")] // Define la ruta base como /api/Metas
    [ApiController]
    [Produces("application/json")] // Especifica que este controlador siempre devolverá JSON
    [Authorize]
    public class MetasController : Controller
    {
        private readonly FinanzasDbContext _context;
        private readonly Services.IMetasService _metasService;

        public MetasController(FinanzasDbContext context, Services.IMetasService metasService)
        {
            _context = context;
            _metasService = metasService;
        }

        // --- ENDPOINTS (MÉTODOS) ---

        /// <summary>
        /// Obtiene una lista completa de todas las metas registradas.
        /// </summary>
        /// <returns>Una lista de objetos Meta.</returns>
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)] // Devuelve 200 si es exitoso
        public async Task<ActionResult<IEnumerable<Meta>>> GetMetas()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var metasDelUsuario = await _context.Metas
                                        .Where(g => g.UserId == userId)
                                        .ToListAsync();

            return Ok(metasDelUsuario);
        }

        /// <summary>
        /// Obtiene una meta específico por su ID.
        /// </summary>
        /// <param name="id">El ID único de la meta a buscar.</param>
        /// <returns>El objeto Meta correspondiente, o un 404 si no se encuentra.</returns>
        [HttpGet("{id}")] // Define la ruta como /api/Metas/5
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)] // Devuelve 404 si no lo encuentra
        public async Task<ActionResult<Meta>> GetMeta(int id)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            // Busca una meta que coincida con el ID Y que pertenezca al usuario
            var meta = await _context.Metas
                                .FirstOrDefaultAsync(g => g.Id == id && g.UserId == userId);

            if (meta == null)
            {
                // Si no existe, o no es de este usuario, devuelve 404
                return NotFound();
            }

            return Ok(meta);
        }

        /// <summary>
        /// Registra una nueva meta en el sistema.
        /// </summary>
        /// <remarks>
        /// Este endpoint reemplaza la macro 'btnGuardar_Click' de Excel.
        /// Ejemplo de JSON que se debe enviar en el cuerpo (body) de la solicitud:
        /// 
        ///     POST /api/Ingresos
        ///     {
        ///       "meta": "Comprar nuevo celular",
        ///       "monto_total": 700,
        ///       "ahorro_actual": 0.00,
        ///       "monto_restante": 700
        ///     }
        ///
        /// </remarks>
        /// <param name="meta">El objeto de Meta a crear desde el cuerpo de la solicitud.</param>
        /// <returns>La mueva meta creada con su ID asignado por la BD.</returns>
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)] // Éxito: Devuelve un 201
        [ProducesResponseType(StatusCodes.Status400BadRequest)] // Falla: Si los datos del modelo son incorrectos
        public async Task<ActionResult<Meta>> PostMeta(Meta meta)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            // --- ¡IMPORTANTE! Asignamos la meta al usuario logueado ---
            meta.UserId = userId;

            _context.Metas.Add(meta);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetMeta", new { id = meta.Id }, meta);
        }

        /// <summary>
        /// Actualiza una meta existente usando su ID.
        /// </summary>
        /// <param name="id">El ID de la meta que se desea modificar.</param>
        /// <param name="meta">El objeto Mete con la información actualizada.</param>
        /// <returns>Un código 204 (Sin Contenido) si la actualización fue exitosa.</returns>
        [HttpPut("{id}")] // Define la ruta como PUT /api/Metas/5
        [ProducesResponseType(StatusCodes.Status204NoContent)] // Éxito: 204
        [ProducesResponseType(StatusCodes.Status400BadRequest)] // Falla: IDs no coinciden
        [ProducesResponseType(StatusCodes.Status404NotFound)] // Falla: Ingreso no existe
        public async Task<IActionResult> PutMeta(int id, Meta meta)
        {
            if (id != meta.Id)
            {
                return BadRequest("El ID de la URL no coincide con el ID del cuerpo.");
            }

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            // Asegurarnos de que el UserId en el objeto a guardar sea el del usuario logueado
            meta.UserId = userId;

            // Verificamos si la meta que intenta modificar realmente le pertenece
            var metaExistente = await _context.Metas
                                        .AsNoTracking() // No lo rastreamos, solo lo leemos
                                        .FirstOrDefaultAsync(g => g.Id == id && g.UserId == userId);

            if (metaExistente == null)
            {
                // Intenta modificar una meta que no existe O no es suyo
                return NotFound();
            }

            // Ahora sí, marcamos la entidad 'meta' (que tiene los datos nuevos) como modificada
            _context.Entry(meta).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!_context.Metas.Any(e => e.Id == id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }

        /// <summary>
        /// Elimina la meta del sistema por su ID.
        /// </summary>
        /// <param name="id">El ID de la meta a eliminar.</param>
        /// <returns>Un código 204 (Sin Contenido) si la eliminación fue exitosa.</returns>
        [HttpDelete("{id}")] // Define la ruta como DELETE /api/Metas/5
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> DeleteMeta(int id)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            // Buscamos la meta que coincida con el ID Y que sea del usuario
            var meta = await _context.Metas
                                .FirstOrDefaultAsync(g => g.Id == id && g.UserId == userId);

            if (meta == null)
            {
                // No existe O no es suyo
                return NotFound();
            }

            _context.Metas.Remove(meta);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        /// <summary>
        /// Obtiene el progreso detallado de una meta específica
        /// </summary>
        /// <param name="id">ID de la meta</param>
        [HttpGet("{id}/progreso")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<object>> GetProgresoMeta(int id)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            try
            {
                var progreso = await _metasService.CalcularProgresoAsync(id, userId!);
                return Ok(progreso);
            }
            catch (KeyNotFoundException)
            {
                return NotFound("Meta no encontrada");
            }
        }

        /// <summary>
        /// Registra un abono manual a una meta
        /// </summary>
        /// <param name="id">ID de la meta</param>
        /// <param name="request">Objeto con el monto a abonar</param>
        [HttpPost("{id}/abonar")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> AbonarMeta(int id, [FromBody] AbonoRequest request)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            try
            {
                var resultado = await _metasService.AbonarMetaAsync(id, userId!, request.Monto);

                if (!resultado)
                    return NotFound("Meta no encontrada");

                return Ok(new { mensaje = "Abono registrado exitosamente", monto = request.Monto });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        /// <summary>
        /// Obtiene proyecciones de cumplimiento de todas las metas del usuario
        /// </summary>
        [HttpGet("proyecciones")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult<List<object>>> GetProyecciones()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var proyecciones = await _metasService.ObtenerProyeccionesAsync(userId!);
            return Ok(proyecciones);
        }
    }

    /// <summary>
    /// Clase para recibir el monto de abono
    /// </summary>
    public class AbonoRequest
    {
        public decimal Monto { get; set; }
    }
}
