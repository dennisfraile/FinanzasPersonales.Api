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
        /// Obtiene una lista de gastos con filtros y paginación.
        /// </summary>
        /// <param name="categoriaId">Filtrar por categoría específica</param>
        /// <param name="tipo">Filtrar por tipo: Fijo o Variable</param>
        /// <param name="desde">Fecha inicial del rango</param>
        /// <param name="hasta">Fecha final del rango</param>
        /// <param name="montoMin">Monto mínimo</param>
        /// <param name="montoMax">Monto máximo</param>
        /// <param name="descripcionContiene">Buscar en descripción</param>
        /// <param name="ordenarPor">Campo para ordenar: fecha o monto. Default: fecha</param>
        /// <param name="ordenDireccion">Dirección de ordenamiento: asc o desc. Default: desc</param>
        /// <param name="pagina">Número de página. Default: 1</param>
        /// <param name="tamañoPagina">Elementos por página. Default: 50, Máximo: 100</param>
        /// <returns>Lista paginada de gastos</returns>
        [HttpGet]
        [ProducesResponseType(typeof(PaginatedResponseDto<Gasto>), StatusCodes.Status200OK)]
        public async Task<ActionResult<PaginatedResponseDto<Gasto>>> GetGastos(
            [FromQuery] int? categoriaId = null,
            [FromQuery] string? tipo = null,
            [FromQuery] DateTime? desde = null,
            [FromQuery] DateTime? hasta = null,
            [FromQuery] decimal? montoMin = null,
            [FromQuery] decimal? montoMax = null,
            [FromQuery] string? descripcionContiene = null,
            [FromQuery] string ordenarPor = "fecha",
            [FromQuery] string ordenDireccion = "desc",
            [FromQuery] int pagina = 1,
            [FromQuery] int tamañoPagina = 50)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            // Limitar tamaño de página
            tamañoPagina = Math.Min(tamañoPagina, 100);
            pagina = Math.Max(pagina, 1);

            var query = _context.Gastos
                .Where(g => g.UserId == userId)
                .Include(g => g.Categoria)
                .AsQueryable();

            // Aplicar filtros
            if (categoriaId.HasValue)
                query = query.Where(g => g.CategoriaId == categoriaId.Value);

            if (!string.IsNullOrEmpty(tipo))
                query = query.Where(g => g.Tipo == tipo);

            if (desde.HasValue)
                query = query.Where(g => g.Fecha >= desde.Value);

            if (hasta.HasValue)
                query = query.Where(g => g.Fecha <= hasta.Value);

            if (montoMin.HasValue)
                query = query.Where(g => g.Monto >= montoMin.Value);

            if (montoMax.HasValue)
                query = query.Where(g => g.Monto <= montoMax.Value);

            if (!string.IsNullOrEmpty(descripcionContiene))
                query = query.Where(g => g.Descripcion != null && g.Descripcion.Contains(descripcionContiene));

            // Ordenamiento
            query = ordenarPor.ToLower() switch
            {
                "monto" => ordenDireccion.ToLower() == "asc"
                    ? query.OrderBy(g => g.Monto)
                    : query.OrderByDescending(g => g.Monto),
                _ => ordenDireccion.ToLower() == "asc"
                    ? query.OrderBy(g => g.Fecha)
                    : query.OrderByDescending(g => g.Fecha)
            };

            // Contar total antes de paginar
            var totalItems = await query.CountAsync();
            var totalPaginas = (int)Math.Ceiling(totalItems / (double)tamañoPagina);

            // Aplicar paginación
            var items = await query
                .Skip((pagina - 1) * tamañoPagina)
                .Take(tamañoPagina)
                .ToListAsync();

            var resultado = new PaginatedResponseDto<Gasto>
            {
                Items = items,
                PaginaActual = pagina,
                TamañoPagina = tamañoPagina,
                TotalItems = totalItems,
                TotalPaginas = totalPaginas,
                TienePaginaAnterior = pagina > 1,
                TienePaginaSiguiente = pagina < totalPaginas
            };

            return Ok(resultado);
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
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            // Busca un gasto que coincida con el ID Y que pertenezca al usuario
            var gasto = await _context.Gastos
                                .FirstOrDefaultAsync(g => g.Id == id && g.UserId == userId);

            if (gasto == null)
            {
                // Si no existe, o no es de este usuario, devuelve 404
                return NotFound();
            }

            return Ok(gasto);
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
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            // --- ¡IMPORTANTE! Asignamos el gasto al usuario logueado ---
            gasto.UserId = userId;

            _context.Gastos.Add(gasto);
            await _context.SaveChangesAsync();

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
                return BadRequest("El ID de la URL no coincide con el ID del cuerpo.");
            }

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            // Asegurarnos de que el UserId en el objeto a guardar sea el del usuario logueado
            gasto.UserId = userId;

            // Verificamos si el gasto que intenta modificar realmente le pertenece
            var gastoExistente = await _context.Gastos
                                        .AsNoTracking() // No lo rastreamos, solo lo leemos
                                        .FirstOrDefaultAsync(g => g.Id == id && g.UserId == userId);

            if (gastoExistente == null)
            {
                // Intenta modificar un gasto que no existe O no es suyo
                return NotFound();
            }

            // Ahora sí, marcamos la entidad 'gasto' (que tiene los datos nuevos) como modificada
            _context.Entry(gasto).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!_context.Gastos.Any(e => e.Id == id))
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
        /// Elimina un gasto del sistema por su ID.
        /// </summary>
        /// <param name="id">El ID del gasto a eliminar.</param>
        /// <returns>Un código 204 (Sin Contenido) si la eliminación fue exitosa.</returns>
        [HttpDelete("{id}")] // Define la ruta como DELETE /api/Gastos/5
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> DeleteGasto(int id)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            // Buscamos el gasto que coincida con el ID Y que sea del usuario
            var gasto = await _context.Gastos
                                .FirstOrDefaultAsync(g => g.Id == id && g.UserId == userId);

            if (gasto == null)
            {
                // No existe O no es suyo
                return NotFound();
            }

            _context.Gastos.Remove(gasto);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}
