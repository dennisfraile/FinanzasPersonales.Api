using Microsoft.EntityFrameworkCore;
using FinanzasPersonales.Api.Data;
using FinanzasPersonales.Api.Dtos;
using FinanzasPersonales.Api.Models;

namespace FinanzasPersonales.Api.Services
{
    public class GastosRecurrentesService : IGastosRecurrentesService
    {
        private readonly FinanzasDbContext _context;

        public GastosRecurrentesService(FinanzasDbContext context)
        {
            _context = context;
        }

        public async Task<List<GastoRecurrenteDto>> GetGastosRecurrentesAsync(string userId)
        {
            return await _context.GastosRecurrentes
                .Include(gr => gr.Categoria)
                .Include(gr => gr.Cuenta)
                .Where(gr => gr.UserId == userId)
                .OrderBy(gr => gr.ProximaFecha)
                .Select(gr => new GastoRecurrenteDto
                {
                    Id = gr.Id,
                    Descripcion = gr.Descripcion,
                    CategoriaId = gr.CategoriaId,
                    CategoriaNombre = gr.Categoria != null ? gr.Categoria.Nombre : null,
                    Monto = gr.Monto,
                    CuentaId = gr.CuentaId,
                    CuentaNombre = gr.Cuenta != null ? gr.Cuenta.Nombre : null,
                    Frecuencia = gr.Frecuencia,
                    DiaDePago = gr.DiaDePago,
                    ProximaFecha = gr.ProximaFecha,
                    UltimaGeneracion = gr.UltimaGeneracion,
                    Activo = gr.Activo,
                    FechaCreacion = gr.FechaCreacion
                })
                .ToListAsync();
        }

        public async Task<GastoRecurrenteDto?> GetGastoRecurrenteAsync(string userId, int id)
        {
            var recurrente = await _context.GastosRecurrentes
                .Include(gr => gr.Categoria)
                .Include(gr => gr.Cuenta)
                .FirstOrDefaultAsync(gr => gr.Id == id && gr.UserId == userId);

            if (recurrente == null)
                return null;

            return new GastoRecurrenteDto
            {
                Id = recurrente.Id,
                Descripcion = recurrente.Descripcion,
                CategoriaId = recurrente.CategoriaId,
                CategoriaNombre = recurrente.Categoria?.Nombre,
                Monto = recurrente.Monto,
                CuentaId = recurrente.CuentaId,
                CuentaNombre = recurrente.Cuenta?.Nombre,
                Frecuencia = recurrente.Frecuencia,
                DiaDePago = recurrente.DiaDePago,
                ProximaFecha = recurrente.ProximaFecha,
                UltimaGeneracion = recurrente.UltimaGeneracion,
                Activo = recurrente.Activo,
                FechaCreacion = recurrente.FechaCreacion
            };
        }

        public async Task<(GastoRecurrenteDto? result, string? error)> CreateGastoRecurrenteAsync(string userId, CreateGastoRecurrenteDto dto)
        {
            var categoria = await _context.Categorias.FindAsync(dto.CategoriaId);
            if (categoria == null || categoria.UserId != userId)
                return (null, "Categoría no válida");

            if (dto.CuentaId.HasValue)
            {
                var cuenta = await _context.Cuentas.FindAsync(dto.CuentaId.Value);
                if (cuenta == null || cuenta.UserId != userId)
                    return (null, "Cuenta no válida");
            }

            var proximaFecha = CalcularProximaFecha(dto.Frecuencia, dto.DiaDePago);

            var recurrente = new GastoRecurrente
            {
                UserId = userId,
                Descripcion = dto.Descripcion,
                CategoriaId = dto.CategoriaId,
                Monto = dto.Monto,
                CuentaId = dto.CuentaId,
                Frecuencia = dto.Frecuencia,
                DiaDePago = dto.DiaDePago,
                ProximaFecha = proximaFecha,
                Activo = true
            };

            _context.GastosRecurrentes.Add(recurrente);
            await _context.SaveChangesAsync();

            await _context.Entry(recurrente).Reference(gr => gr.Categoria).LoadAsync();
            if (recurrente.CuentaId.HasValue)
                await _context.Entry(recurrente).Reference(gr => gr.Cuenta).LoadAsync();

            var result = new GastoRecurrenteDto
            {
                Id = recurrente.Id,
                Descripcion = recurrente.Descripcion,
                CategoriaId = recurrente.CategoriaId,
                CategoriaNombre = recurrente.Categoria?.Nombre,
                Monto = recurrente.Monto,
                CuentaId = recurrente.CuentaId,
                CuentaNombre = recurrente.Cuenta?.Nombre,
                Frecuencia = recurrente.Frecuencia,
                DiaDePago = recurrente.DiaDePago,
                ProximaFecha = recurrente.ProximaFecha,
                UltimaGeneracion = recurrente.UltimaGeneracion,
                Activo = recurrente.Activo,
                FechaCreacion = recurrente.FechaCreacion
            };

            return (result, null);
        }

        public async Task<(bool success, string? error)> UpdateGastoRecurrenteAsync(string userId, int id, UpdateGastoRecurrenteDto dto)
        {
            var recurrente = await _context.GastosRecurrentes.FindAsync(id);
            if (recurrente == null || recurrente.UserId != userId)
                return (false, null);

            var categoria = await _context.Categorias.FindAsync(dto.CategoriaId);
            if (categoria == null || categoria.UserId != userId)
                return (false, "Categoría no válida");

            if (dto.CuentaId.HasValue)
            {
                var cuenta = await _context.Cuentas.FindAsync(dto.CuentaId.Value);
                if (cuenta == null || cuenta.UserId != userId)
                    return (false, "Cuenta no válida");
            }

            recurrente.Descripcion = dto.Descripcion;
            recurrente.CategoriaId = dto.CategoriaId;
            recurrente.Monto = dto.Monto;
            recurrente.CuentaId = dto.CuentaId;
            recurrente.Frecuencia = dto.Frecuencia;
            recurrente.DiaDePago = dto.DiaDePago;
            recurrente.Activo = dto.Activo;

            recurrente.ProximaFecha = CalcularProximaFecha(dto.Frecuencia, dto.DiaDePago);

            await _context.SaveChangesAsync();

            return (true, null);
        }

        public async Task<bool> DeleteGastoRecurrenteAsync(string userId, int id)
        {
            var recurrente = await _context.GastosRecurrentes.FindAsync(id);
            if (recurrente == null || recurrente.UserId != userId)
                return false;

            _context.GastosRecurrentes.Remove(recurrente);
            await _context.SaveChangesAsync();

            return true;
        }

        public async Task<(Gasto? result, string? error)> GenerarGastoAsync(string userId, int id)
        {
            var recurrente = await _context.GastosRecurrentes
                .Include(gr => gr.Cuenta)
                .FirstOrDefaultAsync(gr => gr.Id == id && gr.UserId == userId);

            if (recurrente == null)
                return (null, null); // not found

            if (!recurrente.Activo)
                return (null, "El gasto recurrente está inactivo");

            var gasto = new Gasto
            {
                UserId = userId,
                CategoriaId = recurrente.CategoriaId,
                Descripcion = recurrente.Descripcion + " (Recurrente)",
                Monto = recurrente.Monto,
                CuentaId = recurrente.CuentaId,
                Fecha = DateTime.UtcNow,
                Tipo = "Fijo"
            };

            _context.Gastos.Add(gasto);

            if (recurrente.CuentaId.HasValue && recurrente.Cuenta != null)
            {
                recurrente.Cuenta.BalanceActual -= recurrente.Monto;
            }

            recurrente.UltimaGeneracion = DateTime.UtcNow;
            recurrente.ProximaFecha = CalcularProximaFechaDesde(recurrente.ProximaFecha, recurrente.Frecuencia);

            await _context.SaveChangesAsync();

            return (gasto, null);
        }

        public async Task<int> GenerarPendientesAsync(string userId)
        {
            var pendientes = await _context.GastosRecurrentes
                .Include(gr => gr.Cuenta)
                .Where(gr => gr.UserId == userId && gr.Activo && gr.ProximaFecha <= DateTime.UtcNow)
                .ToListAsync();

            int generados = 0;

            foreach (var recurrente in pendientes)
            {
                var gasto = new Gasto
                {
                    UserId = userId,
                    CategoriaId = recurrente.CategoriaId,
                    Descripcion = recurrente.Descripcion + " (Recurrente)",
                    Monto = recurrente.Monto,
                    CuentaId = recurrente.CuentaId,
                    Fecha = recurrente.ProximaFecha,
                    Tipo = "Fijo"
                };

                _context.Gastos.Add(gasto);

                if (recurrente.CuentaId.HasValue && recurrente.Cuenta != null)
                {
                    recurrente.Cuenta.BalanceActual -= recurrente.Monto;
                }

                recurrente.UltimaGeneracion = DateTime.UtcNow;
                recurrente.ProximaFecha = CalcularProximaFechaDesde(recurrente.ProximaFecha, recurrente.Frecuencia);

                generados++;
            }

            await _context.SaveChangesAsync();

            return generados;
        }

        private DateTime CalcularProximaFecha(string frecuencia, int dia)
        {
            var ahora = DateTime.UtcNow;
            var baseDate = new DateTime(ahora.Year, ahora.Month, Math.Min(dia, DateTime.DaysInMonth(ahora.Year, ahora.Month)), 0, 0, 0, DateTimeKind.Utc);

            return frecuencia switch
            {
                "Semanal" => ahora.AddDays((7 + (dia - (int)ahora.DayOfWeek)) % 7),
                "Quincenal" => baseDate.AddDays(15),
                "Mensual" => baseDate.AddMonths(1),
                "Anual" => baseDate.AddYears(1),
                _ => ahora.AddMonths(1)
            };
        }

        private DateTime CalcularProximaFechaDesde(DateTime desde, string frecuencia)
        {
            return frecuencia switch
            {
                "Semanal" => desde.AddDays(7),
                "Quincenal" => desde.AddDays(15),
                "Mensual" => desde.AddMonths(1),
                "Anual" => desde.AddYears(1),
                _ => desde.AddMonths(1)
            };
        }
    }
}
