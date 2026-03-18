using Microsoft.EntityFrameworkCore;
using FinanzasPersonales.Api.Data;
using FinanzasPersonales.Api.Dtos;
using FinanzasPersonales.Api.Models;

namespace FinanzasPersonales.Api.Services
{
    public class IngresosService : IIngresosService
    {
        private readonly FinanzasDbContext _context;

        public IngresosService(FinanzasDbContext context)
        {
            _context = context;
        }

        public async Task<PaginatedResponseDto<IngresoDto>> GetIngresosAsync(
            string userId,
            int? categoriaId = null,
            DateTime? desde = null,
            DateTime? hasta = null,
            decimal? montoMin = null,
            decimal? montoMax = null,
            string ordenarPor = "fecha",
            string ordenDireccion = "desc",
            int pagina = 1,
            int tamañoPagina = 50,
            List<int>? tagIds = null)
        {
            tamañoPagina = Math.Min(tamañoPagina, 100);
            pagina = Math.Max(pagina, 1);

            var query = _context.Ingresos
                .Where(i => i.UserId == userId)
                .Include(i => i.Categoria)
                .Include(i => i.Cuenta)
                .Include(i => i.IngresoTags)
                    .ThenInclude(it => it.Tag)
                .AsQueryable();

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

            if (tagIds != null && tagIds.Any())
            {
                query = query.Where(i => i.IngresoTags.Any(it => tagIds.Contains(it.TagId)));
            }

            query = ordenarPor.ToLower() switch
            {
                "monto" => ordenDireccion.ToLower() == "asc"
                    ? query.OrderBy(i => i.Monto)
                    : query.OrderByDescending(i => i.Monto),
                _ => ordenDireccion.ToLower() == "asc"
                    ? query.OrderBy(i => i.Fecha).ThenBy(i => i.Id)
                    : query.OrderByDescending(i => i.Fecha).ThenByDescending(i => i.Id)
            };

            var totalItems = await query.CountAsync();
            var totalPaginas = (int)Math.Ceiling(totalItems / (double)tamañoPagina);

            var items = await query
                .Skip((pagina - 1) * tamañoPagina)
                .Take(tamañoPagina)
                .Select(i => new IngresoDto
                {
                    Id = i.Id,
                    Fecha = i.Fecha,
                    CategoriaId = i.CategoriaId,
                    CategoriaNombre = i.Categoria != null ? i.Categoria.Nombre : "",
                    Descripcion = i.Descripcion,
                    Monto = i.Monto,
                    CuentaId = i.CuentaId,
                    CuentaNombre = i.Cuenta != null ? i.Cuenta.Nombre : null,
                    Notas = i.Notas,
                    TagIds = i.IngresoTags.Select(it => it.TagId).ToList()
                })
                .ToListAsync();

            return new PaginatedResponseDto<IngresoDto>
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

        public async Task<IngresoDto?> GetIngresoAsync(string userId, int id)
        {
            var ingreso = await _context.Ingresos
                .Include(i => i.Categoria)
                .Include(i => i.Cuenta)
                .Include(i => i.IngresoTags)
                .FirstOrDefaultAsync(i => i.Id == id && i.UserId == userId);

            if (ingreso == null) return null;

            return new IngresoDto
            {
                Id = ingreso.Id,
                Fecha = ingreso.Fecha,
                CategoriaId = ingreso.CategoriaId,
                CategoriaNombre = ingreso.Categoria?.Nombre,
                Descripcion = ingreso.Descripcion,
                Monto = ingreso.Monto,
                CuentaId = ingreso.CuentaId,
                CuentaNombre = ingreso.Cuenta?.Nombre,
                Notas = ingreso.Notas,
                TagIds = ingreso.IngresoTags.Select(it => it.TagId).ToList(),
            };
        }

        public async Task<IngresoDto> CreateIngresoAsync(string userId, CreateIngresoDto dto)
        {
            // Validar que la categoría pertenece al usuario
            var categoria = await _context.Categorias
                .FirstOrDefaultAsync(c => c.Id == dto.CategoriaId && c.UserId == userId);
            if (categoria == null)
                throw new InvalidOperationException("La categoría no existe o no pertenece al usuario.");

            var ingreso = new Ingreso
            {
                Fecha = DateTime.SpecifyKind(dto.Fecha, DateTimeKind.Utc),
                CategoriaId = dto.CategoriaId,
                Descripcion = dto.Descripcion,
                Monto = dto.Monto,
                CuentaId = dto.CuentaId,
                Notas = dto.Notas,
                UserId = userId
            };

            _context.Ingresos.Add(ingreso);

            if (dto.CuentaId.HasValue)
            {
                var cuenta = await _context.Cuentas.FindAsync(dto.CuentaId.Value);
                if (cuenta != null && cuenta.UserId == userId)
                {
                    cuenta.BalanceActual += dto.Monto;
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

                var ingresoTags = tagsValidos.Select(tagId => new IngresoTag
                {
                    IngresoId = ingreso.Id,
                    TagId = tagId
                }).ToList();

                _context.IngresoTags.AddRange(ingresoTags);
                await _context.SaveChangesAsync();
                tagIdsAsociados = tagsValidos;
            }

            return new IngresoDto
            {
                Id = ingreso.Id,
                Fecha = ingreso.Fecha,
                CategoriaId = ingreso.CategoriaId,
                CategoriaNombre = categoria.Nombre,
                Descripcion = ingreso.Descripcion,
                Monto = ingreso.Monto,
                CuentaId = ingreso.CuentaId,
                Notas = ingreso.Notas,
                TagIds = tagIdsAsociados
            };
        }

        public async Task<bool> UpdateIngresoAsync(string userId, int id, UpdateIngresoDto dto)
        {
            var ingresoExistente = await _context.Ingresos
                .FirstOrDefaultAsync(g => g.Id == id && g.UserId == userId);

            if (ingresoExistente == null)
                return false;

            ingresoExistente.Fecha = DateTime.SpecifyKind(dto.Fecha, DateTimeKind.Utc);
            ingresoExistente.CategoriaId = dto.CategoriaId;
            ingresoExistente.Descripcion = dto.Descripcion;
            ingresoExistente.Notas = dto.Notas;

            var montoAnterior = ingresoExistente.Monto;
            var cuentaAnteriorId = ingresoExistente.CuentaId;

            ingresoExistente.Monto = dto.Monto;
            ingresoExistente.CuentaId = dto.CuentaId;

            if (cuentaAnteriorId != dto.CuentaId)
            {
                if (cuentaAnteriorId.HasValue)
                {
                    var cuentaAnterior = await _context.Cuentas.FindAsync(cuentaAnteriorId.Value);
                    if (cuentaAnterior != null && cuentaAnterior.UserId == userId)
                    {
                        cuentaAnterior.BalanceActual -= montoAnterior;
                    }
                }

                if (dto.CuentaId.HasValue)
                {
                    var cuentaNueva = await _context.Cuentas.FindAsync(dto.CuentaId.Value);
                    if (cuentaNueva != null && cuentaNueva.UserId == userId)
                    {
                        cuentaNueva.BalanceActual += dto.Monto;
                    }
                }
            }
            else if (montoAnterior != dto.Monto && dto.CuentaId.HasValue)
            {
                var cuenta = await _context.Cuentas.FindAsync(dto.CuentaId.Value);
                if (cuenta != null && cuenta.UserId == userId)
                {
                    var diferencia = dto.Monto - montoAnterior;
                    cuenta.BalanceActual += diferencia;
                }
            }

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!_context.Ingresos.Any(e => e.Id == id))
                    return false;
                else
                    throw;
            }

            if (dto.TagIds != null)
            {
                var tagsAntiguos = _context.IngresoTags.Where(it => it.IngresoId == id);
                _context.IngresoTags.RemoveRange(tagsAntiguos);

                if (dto.TagIds.Any())
                {
                    var tagsValidos = await _context.Tags
                        .Where(t => t.UserId == userId && dto.TagIds.Contains(t.Id))
                        .Select(t => t.Id)
                        .ToListAsync();

                    if (tagsValidos.Count != dto.TagIds.Distinct().Count())
                        throw new InvalidOperationException("Uno o más tags no existen o no pertenecen al usuario.");

                    var nuevosTags = tagsValidos.Select(tagId => new IngresoTag
                    {
                        IngresoId = id,
                        TagId = tagId
                    }).ToList();

                    _context.IngresoTags.AddRange(nuevosTags);
                }

                await _context.SaveChangesAsync();
            }

            return true;
        }

        public async Task<bool> DeleteIngresoAsync(string userId, int id)
        {
            var ingreso = await _context.Ingresos
                .FirstOrDefaultAsync(g => g.Id == id && g.UserId == userId);

            if (ingreso == null)
                return false;

            if (ingreso.CuentaId.HasValue)
            {
                var cuenta = await _context.Cuentas.FindAsync(ingreso.CuentaId.Value);
                if (cuenta != null && cuenta.UserId == userId)
                {
                    cuenta.BalanceActual -= ingreso.Monto;
                }
            }

            _context.Ingresos.Remove(ingreso);
            await _context.SaveChangesAsync();

            return true;
        }
    }
}
