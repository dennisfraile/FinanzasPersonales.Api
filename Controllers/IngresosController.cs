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
    [Route("api/[controller]")] // Define la ruta base como /api/Gastos
    [ApiController]
    [Produces("application/json")] // Especifica que este controlador siempre devolverá JSON
    [Authorize]
    public class IngresosController : Controller
    {
        private readonly FinanzasDbContext _context;

        public IngresosController(FinanzasDbContext context)
        {
            _context = context;
        }

        // --- ENDPOINTS (MÉTODOS) ---

        /// <summary>
        /// Obtiene una lista completa de todos los ingresos registrados.
        /// </summary>
        /// <returns>Una lista de objetos Ingreso.</returns>
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)] // Devuelve 200 si es exitoso
        public async Task<ActionResult<IEnumerable<Ingreso>>> GetIngresos()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var ingresosDelUsuario = await _context.Ingresos
                                        .Where(g => g.UserId == userId)
                                        .ToListAsync();

            return Ok(ingresosDelUsuario);
        }

        /// <summary>
        /// Obtiene un ingreso específico por su ID.
        /// </summary>
        /// <param name="id">El ID único del ingreso a buscar.</param>
        /// <returns>El objeto Ingreso correspondiente, o un 404 si no se encuentra.</returns>
        [HttpGet("{id}")] // Define la ruta como /api/Ingresos/5
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)] // Devuelve 404 si no lo encuentra
        public async Task<ActionResult<Ingreso>> GetIngreso(int id)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            // Busca un ingreso que coincida con el ID Y que pertenezca al usuario
            var ingreso = await _context.Ingresos
                                .FirstOrDefaultAsync(g => g.Id == id && g.UserId == userId);

            if (ingreso == null)
            {
                // Si no existe, o no es de este usuario, devuelve 404
                return NotFound();
            }

            return Ok(ingreso);
        }

        /// <summary>
        /// Registra un nuevo ingreso en el sistema.
        /// </summary>
        /// <remarks>
        /// Este endpoint reemplaza la macro 'btnGuardar_Click' de Excel.
        /// Ejemplo de JSON que se debe enviar en el cuerpo (body) de la solicitud:
        /// 
        ///     POST /api/Ingresos
        ///     {
        ///       "fecha": "2025-11-06T15:30:00",
        ///       "fuente": "Salario quincena 2",
        ///       "monto": 12.50
        ///     }
        ///
        /// </remarks>
        /// <param name="ingreso">El objeto de Ingreso a crear desde el cuerpo de la solicitud.</param>
        /// <returns>El nuevo ingreso creado con su ID asignado por la BD.</returns>
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)] // Éxito: Devuelve un 201
        [ProducesResponseType(StatusCodes.Status400BadRequest)] // Falla: Si los datos del modelo son incorrectos
        public async Task<ActionResult<Ingreso>> PostIngreso(Ingreso ingreso)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            // --- ¡IMPORTANTE! Asignamos el ingreso al usuario logueado ---
            ingreso.UserId = userId;

            _context.Ingresos.Add(ingreso);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetIngreso", new { id = ingreso.Id }, ingreso);
        }

        /// <summary>
        /// Actualiza un ingreso existente usando su ID.
        /// </summary>
        /// <param name="id">El ID del gasto que se desea modificar.</param>
        /// <param name="ingreso">El objeto Ingreso con la información actualizada.</param>
        /// <returns>Un código 204 (Sin Contenido) si la actualización fue exitosa.</returns>
        [HttpPut("{id}")] // Define la ruta como PUT /api/Ingresos/5
        [ProducesResponseType(StatusCodes.Status204NoContent)] // Éxito: 204
        [ProducesResponseType(StatusCodes.Status400BadRequest)] // Falla: IDs no coinciden
        [ProducesResponseType(StatusCodes.Status404NotFound)] // Falla: Ingreso no existe
        public async Task<IActionResult> PutIngreso(int id, Ingreso ingreso)
        {
            if (id != ingreso.Id)
            {
                return BadRequest("El ID de la URL no coincide con el ID del cuerpo.");
            }

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            // Asegurarnos de que el UserId en el objeto a guardar sea el del usuario logueado
            ingreso.UserId = userId;

            // Verificamos si el ingreso que intenta modificar realmente le pertenece
            var ingresoExistente = await _context.Ingresos
                                        .AsNoTracking() // No lo rastreamos, solo lo leemos
                                        .FirstOrDefaultAsync(g => g.Id == id && g.UserId == userId);

            if (ingresoExistente == null)
            {
                // Intenta modificar un ingreso que no existe O no es suyo
                return NotFound();
            }

            // Ahora sí, marcamos la entidad 'ingreso' (que tiene los datos nuevos) como modificada
            _context.Entry(ingreso).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!_context.Ingresos.Any(e => e.Id == id))
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
        /// Elimina un ingreso del sistema por su ID.
        /// </summary>
        /// <param name="id">El ID del ingreso a eliminar.</param>
        /// <returns>Un código 204 (Sin Contenido) si la eliminación fue exitosa.</returns>
        [HttpDelete("{id}")] // Define la ruta como DELETE /api/Ingresos/5
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> DeleteIngreso(int id)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            // Buscamos el ingreso que coincida con el ID Y que sea del usuario
            var ingreso = await _context.Ingresos
                                .FirstOrDefaultAsync(g => g.Id == id && g.UserId == userId);

            if (ingreso == null)
            {
                // No existe O no es suyo
                return NotFound();
            }

            _context.Ingresos.Remove(ingreso);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}
