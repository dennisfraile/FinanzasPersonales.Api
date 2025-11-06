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
    public class GastosController : ControllerBase
    {
        private readonly FinanzasDbContext _context;

        /// <summary>
        /// Constructor del controlador.
        /// </summary>
        /// <param name="context">El contexto de la base de datos (inyectado por ASP.NET).</param>
        public GastosController(FinanzasDbContext context)
        {
            _context = context;
        }

        // --- ENDPOINTS (MÉTODOS) ---

        /// <summary>
        /// Obtiene una lista completa de todos los gastos registrados.
        /// </summary>
        /// <returns>Una lista de objetos Gasto.</returns>
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)] // Devuelve 200 si es exitoso
        public async Task<ActionResult<IEnumerable<Gasto>>> GetGastos()
        {
            // Devuelve la lista completa de gastos
            return await _context.Gastos.ToListAsync();
        }

        /// <summary>
        /// Obtiene un gasto específico por su ID.
        /// </summary>
        /// <param name="id">El ID único del gasto a buscar.</param>
        /// <returns>El objeto Gasto correspondiente, o un 404 si no se encuentra.</returns>
        [HttpGet("{id}")] // Define la ruta como /api/Gastos/5
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)] // Devuelve 404 si no lo encuentra
        public async Task<ActionResult<Gasto>> GetGasto(int id)
        {
            var gasto = await _context.Gastos.FindAsync(id);

            if (gasto == null)
            {
                return NotFound(); // Devuelve un HTTP 404
            }

            return Ok(gasto); // Devuelve el gasto y un HTTP 200 (Este es el 'Ok' que mencionamos antes)
        }

        /// <summary>
        /// Registra un nuevo gasto en el sistema.
        /// </summary>
        /// <remarks>
        /// Este endpoint reemplaza la macro 'btnGuardar_Click' de Excel.
        /// Ejemplo de JSON que se debe enviar en el cuerpo (body) de la solicitud:
        /// 
        ///     POST /api/Gastos
        ///     {
        ///       "fecha": "2025-11-06T15:30:00",
        ///       "categoria": "Comida",
        ///       "tipo": "Variable",
        ///       "descripcion": "Almuerzo de trabajo",
        ///       "monto": 12.50
        ///     }
        ///
        /// </remarks>
        /// <param name="gasto">El objeto de Gasto a crear desde el cuerpo de la solicitud.</param>
        /// <returns>El nuevo gasto creado con su ID asignado por la BD.</returns>
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)] // Éxito: Devuelve un 201
        [ProducesResponseType(StatusCodes.Status400BadRequest)] // Falla: Si los datos del modelo son incorrectos
        public async Task<ActionResult<Gasto>> PostGasto(Gasto gasto)
        {
            // El atributo [ApiController] automáticamente valida el modelo (gasto).
            // Si falta un [Required] (ej. Monto), devolverá un 400 BadRequest
            // antes de llegar a este código.

            _context.Gastos.Add(gasto); // Agrega el gasto al contexto de EF Core
            await _context.SaveChangesAsync(); // Guarda los cambios en SQL Server

            // Devuelve un código 201 (Created) con la ruta para obtener el nuevo recurso
            // y el objeto 'gasto' recién creado (que ahora incluye su 'Id').
            return CreatedAtAction("GetGasto", new { id = gasto.Id }, gasto);
        }

        /// <summary>
        /// Actualiza un gasto existente usando su ID.
        /// </summary>
        /// <param name="id">El ID del gasto que se desea modificar.</param>
        /// <param name="gasto">El objeto Gasto con la información actualizada.</param>
        /// <returns>Un código 204 (Sin Contenido) si la actualización fue exitosa.</returns>
        [HttpPut("{id}")] // Define la ruta como PUT /api/Gastos/5
        [ProducesResponseType(StatusCodes.Status204NoContent)] // Éxito: 204
        [ProducesResponseType(StatusCodes.Status400BadRequest)] // Falla: IDs no coinciden
        [ProducesResponseType(StatusCodes.Status404NotFound)] // Falla: Gasto no existe
        public async Task<IActionResult> PutGasto(int id, Gasto gasto)
        {
            if (id != gasto.Id)
            {
                return BadRequest("El ID de la URL no coincide con el ID del cuerpo de la solicitud.");
            }

            _context.Entry(gasto).State = EntityState.Modified; // Marca el objeto como "modificado"

            try
            {
                await _context.SaveChangesAsync(); // Intenta guardar en la BD
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!_context.Gastos.Any(e => e.Id == id))
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
        /// Elimina un gasto del sistema por su ID.
        /// </summary>
        /// <param name="id">El ID del gasto a eliminar.</param>
        /// <returns>Un código 204 (Sin Contenido) si la eliminación fue exitosa.</returns>
        [HttpDelete("{id}")] // Define la ruta como DELETE /api/Gastos/5
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> DeleteGasto(int id)
        {
            var gasto = await _context.Gastos.FindAsync(id);
            if (gasto == null)
            {
                return NotFound(); // No se encontró
            }

            _context.Gastos.Remove(gasto); // Marca el objeto para eliminar
            await _context.SaveChangesAsync(); // Ejecuta la eliminación en SQL Server

            return NoContent(); // Éxito
        }
    }
}
