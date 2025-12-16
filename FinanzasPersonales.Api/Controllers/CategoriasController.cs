using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using FinanzasPersonales.Api.Data;
using FinanzasPersonales.Api.Models;
using FinanzasPersonales.Api.Dtos;
using Microsoft.AspNetCore.Authorization; // 1. Importar para [Authorize]
using System.Security.Claims; // 2. Importar para leer el token
using Microsoft.AspNetCore.Http; // 3. Importar para StatusCodes

namespace FinanzasPersonales.Api.Controllers
{
    /// <summary>
    /// API para gestionar las categorías de Ingreso y Gasto de un usuario.
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    [Authorize] // <-- ¡TODO EL CONTROLADOR ESTÁ PROTEGIDO!
    [Produces("application/json")]
    public class CategoriasController : ControllerBase
    {
        private readonly FinanzasDbContext _context;

        public CategoriasController(FinanzasDbContext context)
        {
            _context = context;
        }

        // --- MÉTODOS DEL CONTROLADOR ---

        /// <summary>
        /// Obtiene una lista de TODAS las categorías (Ingresos y Gastos)
        /// pertenecientes al usuario autenticado.
        /// </summary>
        /// <returns>Una lista de las categorías del usuario.</returns>
        [HttpGet]
        [ProducesResponseType(typeof(IEnumerable<Categoria>), StatusCodes.Status200OK)]
        public async Task<ActionResult<IEnumerable<Categoria>>> GetCategorias()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            return await _context.Categorias
                                 .Where(c => c.UserId == userId)
                                 .ToListAsync();
        }

        /// <summary>
        /// Obtiene una categoría específica por su ID.
        /// </summary>
        /// <param name="id">El ID de la categoría a buscar.</param>
        /// <returns>La categoría, o 404 si no se encuentra o no pertenece al usuario.</returns>
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(Categoria), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<Categoria>> GetCategoria(int id)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var categoria = await _context.Categorias
                                        .FirstOrDefaultAsync(c => c.Id == id && c.UserId == userId);

            if (categoria == null)
            {
                return NotFound("La categoría no existe o no pertenece a este usuario.");
            }

            return Ok(categoria);
        }

        /// <summary>
        /// Crea una nueva categoría (Ingreso o Gasto) para el usuario autenticado.
        /// </summary>
        /// <param name="dto">El objeto CreateCategoriaDto con los datos de la categoría a crear.</param>
        /// <returns>La nueva categoría creada con su ID.</returns>
        [HttpPost]
        [ProducesResponseType(typeof(Categoria), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<Categoria>> PostCategoria(CreateCategoriaDto dto)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrEmpty(userId))
            {
                return BadRequest("No se pudo obtener el UserId del token JWT");
            }

            // Verificar que el usuario existe
            var userExists = await _context.Users.AnyAsync(u => u.Id == userId);
            if (!userExists)
            {
                return BadRequest($"El usuario con ID {userId} no existe en la base de datos");
            }

            var categoria = new Categoria
            {
                Nombre = dto.Nombre,
                Tipo = dto.Tipo,
                UserId = userId
            };

            _context.Categorias.Add(categoria);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetCategoria", new { id = categoria.Id }, categoria);
        }

        /// <summary>
        /// Actualiza el nombre o tipo de una categoría existente.
        /// </summary>
        /// <param name="id">El ID de la categoría a modificar.</param>
        /// <param name="categoria">El objeto Categoría con los datos actualizados.</param>
        /// <returns>Un 204 (Sin Contenido) si fue exitoso.</returns>
        [HttpPut("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> PutCategoria(int id, UpdateCategoriaDto dto)
        {
            if (id != dto.Id)
            {
                return BadRequest("El ID de la URL no coincide con el ID del objeto.");
            }

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var categoriaExistente = await _context.Categorias
                                        .FirstOrDefaultAsync(c => c.Id == id && c.UserId == userId);

            if (categoriaExistente == null)
            {
                return NotFound("La categoría no existe o no pertenece a este usuario.");
            }

            // Actualizamos solo los campos permitidos
            categoriaExistente.Nombre = dto.Nombre;
            categoriaExistente.Tipo = dto.Tipo;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!_context.Categorias.Any(e => e.Id == id))
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
        /// Elimina una categoría del usuario.
        /// </summary>
        /// <remarks>
        /// NOTA: Esto fallará si la categoría ya está siendo usada por algún Gasto o Ingreso
        /// (debido a la regla 'ON DELETE RESTRICT' que configuramos en el DbContext).
        /// </remarks>
        /// <param name="id">El ID de la categoría a eliminar.</param>
        /// <returns>Un 204 (Sin Contenido) si fue exitoso.</returns>
        [HttpDelete("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> DeleteCategoria(int id)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var categoria = await _context.Categorias
                                        .FirstOrDefaultAsync(c => c.Id == id && c.UserId == userId);

            if (categoria == null)
            {
                return NotFound("La categoría no existe o no pertenece a este usuario.");
            }

            try
            {
                _context.Categorias.Remove(categoria);
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException) // Captura el error de la BD
            {
                // Esto ocurre si la regla 'ON DELETE RESTRICT' se activa
                return BadRequest("No se puede eliminar la categoría porque ya está siendo utilizada por gastos o ingresos registrados.");
            }

            return NoContent();
        }
    }
}
