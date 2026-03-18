using Microsoft.EntityFrameworkCore;
using FinanzasPersonales.Api.Data;
using FinanzasPersonales.Api.Dtos;
using FinanzasPersonales.Api.Models;

namespace FinanzasPersonales.Api.Services
{
    /// <summary>
    /// Implementación del servicio de detalles de gasto (sub-compras).
    /// </summary>
    public class DetallesGastoService : IDetallesGastoService
    {
        private readonly FinanzasDbContext _context;

        public DetallesGastoService(FinanzasDbContext context)
        {
            _context = context;
        }

        public async Task<List<DetalleGastoDto>> GetDetallesAsync(string userId, int gastoId)
        {
            var gastoExiste = await _context.Gastos
                .AnyAsync(g => g.Id == gastoId && g.UserId == userId);
            if (!gastoExiste)
                throw new InvalidOperationException("Recurso no encontrado o acceso denegado.");

            return await _context.DetallesGasto
                .Where(d => d.GastoId == gastoId && d.UserId == userId)
                .OrderByDescending(d => d.Fecha)
                .Select(d => new DetalleGastoDto
                {
                    Id = d.Id,
                    GastoId = d.GastoId,
                    Descripcion = d.Descripcion,
                    Monto = d.Monto,
                    Fecha = d.Fecha,
                    Notas = d.Notas
                })
                .ToListAsync();
        }

        public async Task<GastoConDetallesDto?> GetGastoConDetallesAsync(string userId, int gastoId)
        {
            var gasto = await _context.Gastos
                .Include(g => g.Categoria)
                .Include(g => g.GastoTags)
                .Include(g => g.Detalles)
                .FirstOrDefaultAsync(g => g.Id == gastoId && g.UserId == userId);

            if (gasto == null)
                return null;

            var montoConsumido = gasto.Detalles.Sum(d => d.Monto);

            return new GastoConDetallesDto
            {
                Id = gasto.Id,
                Fecha = gasto.Fecha,
                CategoriaId = gasto.CategoriaId,
                CategoriaNombre = gasto.Categoria?.Nombre,
                Tipo = gasto.Tipo ?? "Variable",
                Descripcion = gasto.Descripcion,
                Monto = gasto.Monto,
                CuentaId = gasto.CuentaId,
                Notas = gasto.Notas,
                TagIds = gasto.GastoTags.Select(gt => gt.TagId).ToList(),
                CantidadDetalles = gasto.Detalles.Count,
                MontoConsumido = montoConsumido,
                MontoDisponible = gasto.Monto - montoConsumido,
                Detalles = gasto.Detalles
                    .OrderByDescending(d => d.Fecha)
                    .Select(d => new DetalleGastoDto
                    {
                        Id = d.Id,
                        GastoId = d.GastoId,
                        Descripcion = d.Descripcion,
                        Monto = d.Monto,
                        Fecha = d.Fecha,
                        Notas = d.Notas
                    })
                    .ToList()
            };
        }

        public async Task<DetalleGastoDto> CreateDetalleAsync(string userId, int gastoId, CreateDetalleGastoDto dto)
        {
            var gasto = await _context.Gastos
                .FirstOrDefaultAsync(g => g.Id == gastoId && g.UserId == userId);
            if (gasto == null)
                throw new InvalidOperationException("Recurso no encontrado o acceso denegado.");

            var sumaExistente = await _context.DetallesGasto
                .Where(d => d.GastoId == gastoId)
                .SumAsync(d => (decimal?)d.Monto) ?? 0;

            var disponible = gasto.Monto - sumaExistente;
            if (dto.Monto > disponible)
                throw new InvalidOperationException($"El monto excede el disponible del gasto. Disponible: {disponible:F2}");

            var detalle = new DetalleGasto
            {
                GastoId = gastoId,
                UserId = userId,
                Descripcion = dto.Descripcion,
                Monto = dto.Monto,
                Fecha = DateTime.SpecifyKind(dto.Fecha, DateTimeKind.Utc),
                Notas = dto.Notas
            };

            _context.DetallesGasto.Add(detalle);
            await _context.SaveChangesAsync();

            return new DetalleGastoDto
            {
                Id = detalle.Id,
                GastoId = detalle.GastoId,
                Descripcion = detalle.Descripcion,
                Monto = detalle.Monto,
                Fecha = detalle.Fecha,
                Notas = detalle.Notas
            };
        }

        public async Task<bool> UpdateDetalleAsync(string userId, int gastoId, int detalleId, CreateDetalleGastoDto dto)
        {
            var gasto = await _context.Gastos
                .FirstOrDefaultAsync(g => g.Id == gastoId && g.UserId == userId);
            if (gasto == null)
                return false;

            var detalle = await _context.DetallesGasto
                .FirstOrDefaultAsync(d => d.Id == detalleId && d.GastoId == gastoId && d.UserId == userId);
            if (detalle == null)
                return false;

            // Calcular suma excluyendo el detalle actual
            var sumaOtros = await _context.DetallesGasto
                .Where(d => d.GastoId == gastoId && d.Id != detalleId)
                .SumAsync(d => (decimal?)d.Monto) ?? 0;

            var disponible = gasto.Monto - sumaOtros;
            if (dto.Monto > disponible)
                throw new InvalidOperationException($"El monto excede el disponible del gasto. Disponible: {disponible:F2}");

            detalle.Descripcion = dto.Descripcion;
            detalle.Monto = dto.Monto;
            detalle.Fecha = DateTime.SpecifyKind(dto.Fecha, DateTimeKind.Utc);
            detalle.Notas = dto.Notas;

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeleteDetalleAsync(string userId, int gastoId, int detalleId)
        {
            var detalle = await _context.DetallesGasto
                .FirstOrDefaultAsync(d => d.Id == detalleId && d.GastoId == gastoId && d.UserId == userId);
            if (detalle == null)
                return false;

            _context.DetallesGasto.Remove(detalle);
            await _context.SaveChangesAsync();
            return true;
        }
    }
}
