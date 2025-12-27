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
    public class IngresosController : Controller
    {
        private readonly FinanzasDbContext _context;

        public IngresosController(FinanzasDbContext context)
        {
            _context = context;
        }

        // --- ENDPOINTS (MÉTODOS) ---

        /// <summary>
        /// Obtiene una lista de ingresos con filtros y paginación.
        /// </summary>
        /// <param name="categoriaId">Filtrar por categoría específica</param>
        /// <param name="desde">Fecha inicial del rango</param>
        /// <param name="hasta">Fecha final del rango</param>
        /// <param name="montoMin">Monto mínimo</param>
        /// <param name="montoMax">Monto máximo</param>
        /// <param name="ordenarPor">Campo para ordenar: fecha o monto. Default: fecha</param>
        /// <param name="ordenDireccion">Dirección de ordenamiento: asc o desc. Default: desc</param>
        /// <param name="pagina">Número de página. Default: 1</param>
        /// <param name="tamañoPagina">Elementos por página. Default: 50, Máximo: 100</param>
        /// <returns>Lista paginada de ingresos</returns>
        [HttpGet]
        [ProducesResponseType(typeof(PaginatedResponseDto<IngresoDto>), StatusCodes.Status200OK)]
        public async Task<ActionResult<PaginatedResponseDto<IngresoDto>>> GetIngresos(
            [FromQuery] int? categoriaId = null,
            [FromQuery] DateTime? desde = null,
            [FromQuery] DateTime? hasta = null,
            [FromQuery] decimal? montoMin = null,
            [FromQuery] decimal? montoMax = null,
            [FromQuery] string ordenarPor = "fecha",
            [FromQuery] string ordenDireccion = "desc",
            [FromQuery] int pagina = 1,
            [FromQuery] int tamañoPagina = 50)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            // Limitar tamaño de página
            tamañoPagina = Math.Min(tamañoPagina, 100);
            pagina = Math.Max(pagina, 1);

            var query = _context.Ingresos
                .Where(i => i.UserId == userId)
                .Include(i => i.Categoria)
                .AsQueryable();

            // Aplicar filtros
            if (categoriaId.HasValue)
                query = query.Where(i => i.CategoriaId == categoriaId.Value);

            if (desde.HasValue)
                query = query.Where(i => i.Fecha >= desde.Value);

            if (hasta.HasValue)
                query = query.Where(i => i.Fecha <= hasta.Value);

            if (montoMin.HasValue)
                query = query.Where(i => i.Monto >= montoMin.Value);

            if (montoMax.HasValue)
                query = query.Where(i => i.Monto <= montoMax.Value);

            // Ordenamiento
            query = ordenarPor.ToLower() switch
            {
                "monto" => ordenDireccion.ToLower() == "asc"
                    ? query.OrderBy(i => i.Monto)
                    : query.OrderByDescending(i => i.Monto),
                _ => ordenDireccion.ToLower() == "asc"
                    ? query.OrderBy(i => i.Fecha)
                    : query.OrderByDescending(i => i.Fecha)
            };

            // Contar total antes de paginar
            var totalItems = await query.CountAsync();
            var totalPaginas = (int)Math.Ceiling(totalItems / (double)tamañoPagina);

            // Aplicar paginación y mapear a DTO
            var items = await query
                .Skip((pagina - 1) * tamañoPagina)
                .Take(tamañoPagina)
                .Select(i => new IngresoDto
                {
                    Id = i.Id,
                    Fecha = i.Fecha,
                    CategoriaId = i.CategoriaId,
                    CategoriaNombre = i.Categoria != null ? i.Categoria.Nombre : null,
                    Descripcion = i.Descripcion,
                    Monto = i.Monto
                })
                .ToListAsync();

            var resultado = new PaginatedResponseDto<IngresoDto>
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
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<Ingreso>> PostIngreso(CreateIngresoDto dto)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrEmpty(userId))
            {
                return BadRequest("No se pudo obtener el UserId del token JWT");
            }

            // Crear entidad Ingreso desde el DTO
            var ingreso = new Ingreso
            {
                Fecha = DateTime.SpecifyKind(dto.Fecha, DateTimeKind.Utc),
                CategoriaId = dto.CategoriaId,
                Descripcion = dto.Descripcion,
                Monto = dto.Monto,
                CuentaId = dto.CuentaId, // NUEVO
                UserId = userId
            };

            _context.Ingresos.Add(ingreso);

            // NUEVO: Actualizar balance de cuenta (SUMAR)
            if (dto.CuentaId.HasValue)
            {
                var cuenta = await _context.Cuentas.FindAsync(dto.CuentaId.Value);
                if (cuenta != null && cuenta.UserId == userId)
                {
                    cuenta.BalanceActual += dto.Monto; // SUMAR ingreso
                }
            }
            await _context.SaveChangesAsync();

            // Guardar relaciones con tags
            if (dto.TagIds != null && dto.TagIds.Any())
            {
                var ingresoTags = dto.TagIds.Select(tagId => new IngresoTag
                {
                    IngresoId = ingreso.Id,
                    TagId = tagId
                }).ToList();

                _context.IngresoTags.AddRange(ingresoTags);
                await _context.SaveChangesAsync();
            }

            return CreatedAtAction("GetIngreso", new { id = ingreso.Id }, ingreso);
        }

        /// <summary>
        /// Actualiza un ingreso existente usando su ID.
        /// </summary>
        /// <param name="id">El ID del gasto que se desea modificar.</param>
        /// <param name="ingreso">El objeto Ingreso con la información actualizada.</param>
        /// <returns>Un código 204 (Sin Contenido) si la actualización fue exitosa.</returns>
        [HttpPut("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> PutIngreso(int id, UpdateIngresoDto dto)
        {
            if (id != dto.Id)
            {
                return BadRequest("El ID de la URL no coincide con el ID del cuerpo.");
            }

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            // Verificar que el ingreso existe y pertenece al usuario
            var ingresoExistente = await _context.Ingresos
                .FirstOrDefaultAsync(g => g.Id == id && g.UserId == userId);

            if (ingresoExistente == null)
            {
                return NotFound();
            }

            // Actualizar propiedades desde el DTO
            ingresoExistente.Fecha = DateTime.SpecifyKind(dto.Fecha, DateTimeKind.Utc);
            ingresoExistente.CategoriaId = dto.CategoriaId;
            ingresoExistente.Descripcion = dto.Descripcion;

            // NUEVO: Ajustar balances (inverso de gastos - SUMAR)
            var montoAnterior = ingresoExistente.Monto;
            var cuentaAnteriorId = ingresoExistente.CuentaId;

            ingresoExistente.Monto = dto.Monto;
            ingresoExistente.CuentaId = dto.CuentaId;
            // UserId NO se modifica

            // Ajustar balances:
            if (cuentaAnteriorId != dto.CuentaId)
            {
                // Cambió de cuenta
                if (cuentaAnteriorId.HasValue)
                {
                    var cuentaAnterior = await _context.Cuentas.FindAsync(cuentaAnteriorId.Value);
                    if (cuentaAnterior != null && cuentaAnterior.UserId == userId)
                    {
                        cuentaAnterior.BalanceActual -= montoAnterior; // Revertir ingreso anterior
                    }
                }

                if (dto.CuentaId.HasValue)
                {
                    var cuentaNueva = await _context.Cuentas.FindAsync(dto.CuentaId.Value);
                    if (cuentaNueva != null && cuentaNueva.UserId == userId)
                    {
                        cuentaNueva.BalanceActual += dto.Monto; // Aplicar nuevo ingreso
                    }
                }
            }
            else if (montoAnterior != dto.Monto && dto.CuentaId.HasValue)
            {
                // Misma cuenta, cambió monto
                var cuenta = await _context.Cuentas.FindAsync(dto.CuentaId.Value);
                if (cuenta != null && cuenta.UserId == userId)
                {
                    var diferencia = dto.Monto - montoAnterior;
                    cuenta.BalanceActual += diferencia; // SUMAR diferencia
                }
            }

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

            // Actualizar tags: eliminar antiguos y agregar nuevos
            if (dto.TagIds != null)
            {
                // Eliminar tags anteriores
                var tagsAntiguos = _context.IngresoTags.Where(it => it.IngresoId == id);
                _context.IngresoTags.RemoveRange(tagsAntiguos);

                // Agregar nuevos tags
                if (dto.TagIds.Any())
                {
                    var nuevosTags = dto.TagIds.Select(tagId => new IngresoTag
                    {
                        IngresoId = id,
                        TagId = tagId
                    }).ToList();

                    _context.IngresoTags.AddRange(nuevosTags);
                }

                await _context.SaveChangesAsync();
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

            // NUEVO: Revertir balance si tenía cuenta (RESTAR)
            if (ingreso.CuentaId.HasValue)
            {
                var cuenta = await _context.Cuentas.FindAsync(ingreso.CuentaId.Value);
                if (cuenta != null && cuenta.UserId == userId)
                {
                    cuenta.BalanceActual -= ingreso.Monto; // Restar ingreso eliminado
                }
            }

            _context.Ingresos.Remove(ingreso);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}
