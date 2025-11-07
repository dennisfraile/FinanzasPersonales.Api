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

namespace FinanzasPersonales.Api.Controllers
{
    /// <summary>
    /// API para gestionar todos los gastos de la aplicación.
    /// </summary>
    [Route("api/[controller]")] // Define la ruta base como /api/Gastos
    [ApiController]
    [Produces("application/json")] // Especifica que este controlador siempre devolverá JSON
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
            // Devuelve la lista completa de ingresos
            return await _context.Ingresos.ToListAsync();
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
            var ingreso = await _context.Ingresos.FindAsync(id);

            if (ingreso == null)
            {
                return NotFound(); // Devuelve un HTTP 404
            }

            return Ok(ingreso); // Devuelve el ingreso y un HTTP 200 
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
            // El atributo [ApiController] automáticamente valida el modelo (ingreso).
            // Si falta un [Required] (ej. Monto), devolverá un 400 BadRequest
            // antes de llegar a este código.

            _context.Ingresos.Add(ingreso); // Agrega el ingreso al contexto de EF Core
            await _context.SaveChangesAsync(); // Guarda los cambios en SQL Server

            // Devuelve un código 201 (Created) con la ruta para obtener el nuevo recurso
            // y el objeto 'ingreso' recién creado (que ahora incluye su 'Id').
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
                return BadRequest("El ID de la URL no coincide con el ID del cuerpo de la solicitud.");
            }

            _context.Entry(ingreso).State = EntityState.Modified; // Marca el objeto como "modificado"

            try
            {
                await _context.SaveChangesAsync(); // Intenta guardar en la BD
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!_context.Ingresos.Any(e => e.Id == id))
                {
                    return NotFound(); // No se encontró el gasto
                }
                else
                {
                    throw; // Lanza la excepción de concurrencia
                }
            }

            return NoContent(); // Devuelve 204 (No Content)
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
            var ingreso = await _context.Ingresos.FindAsync(id);
            if (ingreso == null)
            {
                return NotFound(); // No se encontró
            }

            _context.Ingresos.Remove(ingreso); // Marca el objeto para eliminar
            await _context.SaveChangesAsync(); // Ejecuta la eliminación en SQL Server

            return NoContent(); // Éxito
        }
    }
}
