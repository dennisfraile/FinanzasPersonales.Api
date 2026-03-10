using Microsoft.EntityFrameworkCore;
using FinanzasPersonales.Api.Data;
using FinanzasPersonales.Api.Dtos;
using FinanzasPersonales.Api.Models;

namespace FinanzasPersonales.Api.Services
{
    public class PresupuestosService : IPresupuestosService
    {
        private readonly FinanzasDbContext _context;

        public PresupuestosService(FinanzasDbContext context)
        {
            _context = context;
        }

        public async Task<List<PresupuestoDto>> GetPresupuestosAsync(string userId, int? mes = null, int? ano = null)
        {
            IQueryable<Presupuesto> query = _context.Presupuestos
                .Where(p => p.UserId == userId);

            if (mes.HasValue)
                query = query.Where(p => p.MesAplicable == mes.Value);

            if (ano.HasValue)
                query = query.Where(p => p.AnoAplicable == ano.Value);

            var presupuestos = await query.Include(p => p.Categoria).ToListAsync();

            var resultado = new List<PresupuestoDto>();

            foreach (var presupuesto in presupuestos)
            {
                var gastadoActual = await CalcularGastadoActual(userId, presupuesto);

                resultado.Add(new PresupuestoDto
                {
                    Id = presupuesto.Id,
                    CategoriaId = presupuesto.CategoriaId,
                    CategoriaNombre = presupuesto.Categoria!.Nombre,
                    MontoLimite = presupuesto.MontoLimite,
                    Periodo = presupuesto.Periodo,
                    MesAplicable = presupuesto.MesAplicable,
                    AnoAplicable = presupuesto.AnoAplicable,
                    GastadoActual = gastadoActual,
                    Disponible = presupuesto.MontoLimite - gastadoActual,
                    PorcentajeUtilizado = presupuesto.MontoLimite > 0
                        ? (gastadoActual / presupuesto.MontoLimite) * 100
                        : 0
                });
            }

            return resultado;
        }

        public async Task<PresupuestoDto?> GetPresupuestoAsync(string userId, int id)
        {
            var presupuesto = await _context.Presupuestos
                .Include(p => p.Categoria)
                .FirstOrDefaultAsync(p => p.Id == id && p.UserId == userId);

            if (presupuesto == null)
                return null;

            var gastadoActual = await CalcularGastadoActual(userId, presupuesto);

            return new PresupuestoDto
            {
                Id = presupuesto.Id,
                CategoriaId = presupuesto.CategoriaId,
                CategoriaNombre = presupuesto.Categoria!.Nombre,
                MontoLimite = presupuesto.MontoLimite,
                Periodo = presupuesto.Periodo,
                MesAplicable = presupuesto.MesAplicable,
                AnoAplicable = presupuesto.AnoAplicable,
                GastadoActual = gastadoActual,
                Disponible = presupuesto.MontoLimite - gastadoActual,
                PorcentajeUtilizado = presupuesto.MontoLimite > 0
                    ? (gastadoActual / presupuesto.MontoLimite) * 100
                    : 0
            };
        }

        public async Task<(PresupuestoDto? result, string? error)> CreatePresupuestoAsync(string userId, PresupuestoCreateDto dto)
        {
            var categoriaExiste = await _context.Categorias
                .AnyAsync(c => c.Id == dto.CategoriaId && c.UserId == userId);

            if (!categoriaExiste)
                return (null, "La categoría no existe o no pertenece al usuario.");

            var presupuestoExiste = await _context.Presupuestos
                .AnyAsync(p => p.UserId == userId &&
                              p.CategoriaId == dto.CategoriaId &&
                              p.MesAplicable == dto.MesAplicable &&
                              p.AnoAplicable == dto.AnoAplicable &&
                              p.Periodo == dto.Periodo);

            if (presupuestoExiste)
                return (null, "Ya existe un presupuesto para esta categoría en el período especificado.");

            var presupuesto = new Presupuesto
            {
                CategoriaId = dto.CategoriaId,
                MontoLimite = dto.MontoLimite,
                Periodo = dto.Periodo,
                MesAplicable = dto.MesAplicable,
                AnoAplicable = dto.AnoAplicable,
                UserId = userId
            };

            _context.Presupuestos.Add(presupuesto);
            await _context.SaveChangesAsync();

            var result = await GetPresupuestoAsync(userId, presupuesto.Id);
            return (result, null);
        }

        public async Task<bool> UpdatePresupuestoAsync(string userId, int id, PresupuestoUpdateDto dto)
        {
            var presupuesto = await _context.Presupuestos
                .FirstOrDefaultAsync(p => p.Id == id && p.UserId == userId);

            if (presupuesto == null)
                return false;

            presupuesto.MontoLimite = dto.MontoLimite;
            presupuesto.Periodo = dto.Periodo;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!await _context.Presupuestos.AnyAsync(p => p.Id == id))
                    return false;
                else
                    throw;
            }

            return true;
        }

        public async Task<bool> DeletePresupuestoAsync(string userId, int id)
        {
            var presupuesto = await _context.Presupuestos
                .FirstOrDefaultAsync(p => p.Id == id && p.UserId == userId);

            if (presupuesto == null)
                return false;

            _context.Presupuestos.Remove(presupuesto);
            await _context.SaveChangesAsync();

            return true;
        }

        public async Task<List<PresupuestoDto>> GetAlertasAsync(string userId)
        {
            var mesActual = DateTime.Now.Month;
            var anoActual = DateTime.Now.Year;

            var presupuestos = await _context.Presupuestos
                .Where(p => p.UserId == userId &&
                           p.MesAplicable == mesActual &&
                           p.AnoAplicable == anoActual)
                .Include(p => p.Categoria)
                .ToListAsync();

            var resultado = new List<PresupuestoDto>();

            foreach (var presupuesto in presupuestos)
            {
                var gastadoActual = await CalcularGastadoActual(userId, presupuesto);
                var porcentaje = presupuesto.MontoLimite > 0
                    ? (gastadoActual / presupuesto.MontoLimite) * 100
                    : 0;

                if (porcentaje >= 80)
                {
                    resultado.Add(new PresupuestoDto
                    {
                        Id = presupuesto.Id,
                        CategoriaId = presupuesto.CategoriaId,
                        CategoriaNombre = presupuesto.Categoria!.Nombre,
                        MontoLimite = presupuesto.MontoLimite,
                        Periodo = presupuesto.Periodo,
                        MesAplicable = presupuesto.MesAplicable,
                        AnoAplicable = presupuesto.AnoAplicable,
                        GastadoActual = gastadoActual,
                        Disponible = presupuesto.MontoLimite - gastadoActual,
                        PorcentajeUtilizado = porcentaje
                    });
                }
            }

            return resultado.OrderByDescending(p => p.PorcentajeUtilizado).ToList();
        }

        public async Task<decimal> CalcularGastadoActual(string userId, Presupuesto presupuesto)
        {
            DateTime inicio, fin;

            if (presupuesto.Periodo == "Mensual")
            {
                inicio = new DateTime(presupuesto.AnoAplicable, presupuesto.MesAplicable, 1);
                fin = inicio.AddMonths(1).AddDays(-1);
            }
            else
            {
                inicio = new DateTime(presupuesto.AnoAplicable, presupuesto.MesAplicable, 1);
                fin = new DateTime(presupuesto.AnoAplicable, presupuesto.MesAplicable, 15);
            }

            inicio = DateTime.SpecifyKind(inicio, DateTimeKind.Utc);
            fin = DateTime.SpecifyKind(fin, DateTimeKind.Utc);

            var gastado = await _context.Gastos
                .Where(g => g.UserId == userId &&
                           g.CategoriaId == presupuesto.CategoriaId &&
                           g.Fecha >= inicio &&
                           g.Fecha <= fin)
                .SumAsync(g => g.Monto);

            return gastado;
        }
    }
}
