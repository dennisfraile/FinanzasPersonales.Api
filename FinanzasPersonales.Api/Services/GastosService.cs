using Microsoft.EntityFrameworkCore;
using FinanzasPersonales.Api.Data;
using FinanzasPersonales.Api.Dtos;
using FinanzasPersonales.Api.Models;

namespace FinanzasPersonales.Api.Services
{
    public class GastosService : IGastosService
    {
        private readonly FinanzasDbContext _context;

        public GastosService(FinanzasDbContext context)
        {
            _context = context;
        }

        public async Task<PaginatedResponseDto<GastoDto>> GetGastosAsync(
            string userId,
            int? categoriaId = null,
            string? tipo = null,
            DateTime? desde = null,
            DateTime? hasta = null,
            decimal? montoMin = null,
            decimal? montoMax = null,
            string? descripcionContiene = null,
            string ordenarPor = "fecha",
            string ordenDireccion = "desc",
            int pagina = 1,
            int tamañoPagina = 50,
            List<int>? tagIds = null)
        {
            tamañoPagina = Math.Min(tamañoPagina, 100);
            pagina = Math.Max(pagina, 1);

            var query = _context.Gastos
                .Where(g => g.UserId == userId)
                .Include(g => g.Categoria)
                .Include(g => g.Cuenta)
                .Include(g => g.GastoTags)
                    .ThenInclude(gt => gt.Tag)
                .Include(g => g.Detalles)
                .AsQueryable();

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

            if (tagIds != null && tagIds.Any())
            {
                query = query.Where(g => g.GastoTags.Any(gt => tagIds.Contains(gt.TagId)));
            }

            query = ordenarPor.ToLower() switch
            {
                "monto" => ordenDireccion.ToLower() == "asc"
                    ? query.OrderBy(g => g.Monto)
                    : query.OrderByDescending(g => g.Monto),
                _ => ordenDireccion.ToLower() == "asc"
                    ? query.OrderBy(g => g.Fecha).ThenBy(g => g.Id)
                    : query.OrderByDescending(g => g.Fecha).ThenByDescending(g => g.Id)
            };

            var totalItems = await query.CountAsync();
            var totalPaginas = (int)Math.Ceiling(totalItems / (double)tamañoPagina);

            var items = await query
                .Skip((pagina - 1) * tamañoPagina)
                .Take(tamañoPagina)
                .Select(g => new GastoDto
                {
                    Id = g.Id,
                    Fecha = g.Fecha,
                    CategoriaId = g.CategoriaId,
                    CategoriaNombre = g.Categoria != null ? g.Categoria.Nombre : "",
                    Tipo = g.Tipo ?? "Variable",
                    Descripcion = g.Descripcion,
                    Monto = g.Monto,
                    CuentaId = g.CuentaId,
                    CuentaNombre = g.Cuenta != null ? g.Cuenta.Nombre : null,
                    Notas = g.Notas,
                    TagIds = g.GastoTags.Select(gt => gt.TagId).ToList(),
                    CantidadDetalles = g.Detalles.Count,
                    MontoDisponible = g.Detalles.Any() ? g.Monto - g.Detalles.Sum(d => d.Monto) : (decimal?)null
                })
                .ToListAsync();

            return new PaginatedResponseDto<GastoDto>
            {
                Items = items,
                PaginaActual = pagina,
                TamañoPagina = tamañoPagina,
                TotalItems = totalItems,
                TotalPaginas = totalPaginas,
                TienePaginaAnterior = pagina > 1,
                TienePaginaSiguiente = pagina < totalPaginas
            };
        }

        public async Task<GastoDto?> GetGastoAsync(string userId, int id)
        {
            var gasto = await _context.Gastos
                .Include(g => g.Categoria)
                .Include(g => g.Cuenta)
                .Include(g => g.GastoTags)
                .Include(g => g.Detalles)
                .FirstOrDefaultAsync(g => g.Id == id && g.UserId == userId);

            if (gasto == null) return null;

            return new GastoDto
            {
                Id = gasto.Id,
                Fecha = gasto.Fecha,
                CategoriaId = gasto.CategoriaId,
                CategoriaNombre = gasto.Categoria?.Nombre,
                Tipo = gasto.Tipo,
                Descripcion = gasto.Descripcion,
                Monto = gasto.Monto,
                CuentaId = gasto.CuentaId,
                CuentaNombre = gasto.Cuenta?.Nombre,
                Notas = gasto.Notas,
                TagIds = gasto.GastoTags.Select(gt => gt.TagId).ToList(),
                CantidadDetalles = gasto.Detalles.Count,
                MontoDisponible = gasto.Monto - gasto.Detalles.Sum(d => d.Monto),
            };
        }

        public async Task<GastoDto> CreateGastoAsync(string userId, CreateGastoDto dto)
        {
            // Validar que la categoría pertenece al usuario
            var categoria = await _context.Categorias
                .FirstOrDefaultAsync(c => c.Id == dto.CategoriaId && c.UserId == userId);
            if (categoria == null)
                throw new InvalidOperationException("Recurso no encontrado o acceso denegado.");

            var gasto = new Gasto
            {
                Fecha = DateTime.SpecifyKind(dto.Fecha, DateTimeKind.Utc),
                CategoriaId = dto.CategoriaId,
                Tipo = dto.Tipo,
                Descripcion = dto.Descripcion,
                Monto = dto.Monto,
                CuentaId = dto.CuentaId,
                Notas = dto.Notas,
                UserId = userId
            };

            _context.Gastos.Add(gasto);

            if (dto.CuentaId.HasValue)
            {
                var cuenta = await _context.Cuentas.FindAsync(dto.CuentaId.Value);
                if (cuenta != null && cuenta.UserId == userId)
                {
                    cuenta.BalanceActual -= dto.Monto;
                }
            }
            await _context.SaveChangesAsync();

            List<int> tagIdsAsociados = new();
            if (dto.TagIds != null && dto.TagIds.Any())
            {
                var tagsValidos = await _context.Tags
                    .Where(t => t.UserId == userId && dto.TagIds.Contains(t.Id))
                    .Select(t => t.Id)
                    .ToListAsync();

                if (tagsValidos.Count != dto.TagIds.Distinct().Count())
                    throw new InvalidOperationException("Uno o más tags no existen o no pertenecen al usuario.");

                var gastoTags = tagsValidos.Select(tagId => new GastoTag
                {
                    GastoId = gasto.Id,
                    TagId = tagId
                }).ToList();

                _context.GastoTags.AddRange(gastoTags);
                await _context.SaveChangesAsync();
                tagIdsAsociados = tagsValidos;
            }

            return new GastoDto
            {
                Id = gasto.Id,
                Fecha = gasto.Fecha,
                CategoriaId = gasto.CategoriaId,
                CategoriaNombre = categoria.Nombre,
                Tipo = gasto.Tipo ?? "Variable",
                Descripcion = gasto.Descripcion,
                Monto = gasto.Monto,
                CuentaId = gasto.CuentaId,
                Notas = gasto.Notas,
                TagIds = tagIdsAsociados
            };
        }

        public async Task<bool> UpdateGastoAsync(string userId, int id, UpdateGastoDto dto)
        {
            var gastoExistente = await _context.Gastos
                .FirstOrDefaultAsync(g => g.Id == id && g.UserId == userId);

            if (gastoExistente == null)
                return false;

            // Validar que no se reduzca el monto por debajo de lo ya consumido en detalles
            if (dto.Monto < gastoExistente.Monto)
            {
                var sumaDetalles = await _context.DetallesGasto
                    .Where(d => d.GastoId == id)
                    .SumAsync(d => (decimal?)d.Monto) ?? 0;

                if (dto.Monto < sumaDetalles)
                    throw new InvalidOperationException($"No puede reducir el monto por debajo de lo ya consumido en compras ({sumaDetalles:F2}).");
            }

            gastoExistente.Fecha = DateTime.SpecifyKind(dto.Fecha, DateTimeKind.Utc);
            gastoExistente.CategoriaId = dto.CategoriaId;
            gastoExistente.Tipo = dto.Tipo;
            gastoExistente.Descripcion = dto.Descripcion;
            gastoExistente.Notas = dto.Notas;

            var montoAnterior = gastoExistente.Monto;
            var cuentaAnteriorId = gastoExistente.CuentaId;

            gastoExistente.Monto = dto.Monto;
            gastoExistente.CuentaId = dto.CuentaId;

            if (cuentaAnteriorId != dto.CuentaId)
            {
                if (cuentaAnteriorId.HasValue)
                {
                    var cuentaAnterior = await _context.Cuentas.FindAsync(cuentaAnteriorId.Value);
                    if (cuentaAnterior != null && cuentaAnterior.UserId == userId)
                    {
                        cuentaAnterior.BalanceActual += montoAnterior;
                    }
                }

                if (dto.CuentaId.HasValue)
                {
                    var cuentaNueva = await _context.Cuentas.FindAsync(dto.CuentaId.Value);
                    if (cuentaNueva != null && cuentaNueva.UserId == userId)
                    {
                        cuentaNueva.BalanceActual -= dto.Monto;
                    }
                }
            }
            else if (montoAnterior != dto.Monto && dto.CuentaId.HasValue)
            {
                var cuenta = await _context.Cuentas.FindAsync(dto.CuentaId.Value);
                if (cuenta != null && cuenta.UserId == userId)
                {
                    var diferencia = dto.Monto - montoAnterior;
                    cuenta.BalanceActual -= diferencia;
                }
            }

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!_context.Gastos.Any(e => e.Id == id))
                    return false;
                else
                    throw;
            }

            if (dto.TagIds != null)
            {
                var tagsAntiguos = _context.GastoTags.Where(gt => gt.GastoId == id);
                _context.GastoTags.RemoveRange(tagsAntiguos);

                if (dto.TagIds.Any())
                {
                    var tagsValidos = await _context.Tags
                        .Where(t => t.UserId == userId && dto.TagIds.Contains(t.Id))
                        .Select(t => t.Id)
                        .ToListAsync();

                    if (tagsValidos.Count != dto.TagIds.Distinct().Count())
                        throw new InvalidOperationException("Uno o más tags no existen o no pertenecen al usuario.");

                    var nuevosTags = tagsValidos.Select(tagId => new GastoTag
                    {
                        GastoId = id,
                        TagId = tagId
                    }).ToList();

                    _context.GastoTags.AddRange(nuevosTags);
                }

                await _context.SaveChangesAsync();
            }

            return true;
        }

        public async Task<bool> DeleteGastoAsync(string userId, int id)
        {
            var gasto = await _context.Gastos
                .FirstOrDefaultAsync(g => g.Id == id && g.UserId == userId);

            if (gasto == null)
                return false;

            if (gasto.CuentaId.HasValue)
            {
                var cuenta = await _context.Cuentas.FindAsync(gasto.CuentaId.Value);
                if (cuenta != null && cuenta.UserId == userId)
                {
                    cuenta.BalanceActual += gasto.Monto;
                }
            }

            _context.Gastos.Remove(gasto);
            await _context.SaveChangesAsync();

            return true;
        }
    }
}
