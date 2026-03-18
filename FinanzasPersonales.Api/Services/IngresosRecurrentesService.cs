using Microsoft.EntityFrameworkCore;
using FinanzasPersonales.Api.Data;
using FinanzasPersonales.Api.Dtos;
using FinanzasPersonales.Api.Models;

namespace FinanzasPersonales.Api.Services
{
    public class IngresosRecurrentesService : IIngresosRecurrentesService
    {
        private readonly FinanzasDbContext _context;

        public IngresosRecurrentesService(FinanzasDbContext context)
        {
            _context = context;
        }

        public async Task<List<IngresoRecurrenteDto>> GetIngresosRecurrentesAsync(string userId)
        {
            return await _context.IngresosRecurrentes
                .Include(ir => ir.Categoria)
                .Include(ir => ir.Cuenta)
                .Where(ir => ir.UserId == userId)
                .OrderBy(ir => ir.ProximaFecha)
                .Select(ir => new IngresoRecurrenteDto
                {
                    Id = ir.Id,
                    Descripcion = ir.Descripcion,
                    CategoriaId = ir.CategoriaId,
                    CategoriaNombre = ir.Categoria != null ? ir.Categoria.Nombre : null,
                    Monto = ir.Monto,
                    CuentaId = ir.CuentaId,
                    CuentaNombre = ir.Cuenta != null ? ir.Cuenta.Nombre : null,
                    Frecuencia = ir.Frecuencia,
                    DiaDePago = ir.DiaDePago,
                    ProximaFecha = ir.ProximaFecha,
                    UltimaGeneracion = ir.UltimaGeneracion,
                    Activo = ir.Activo,
                    FechaCreacion = ir.FechaCreacion
                })
                .ToListAsync();
        }

        public async Task<IngresoRecurrenteDto?> GetIngresoRecurrenteAsync(string userId, int id)
        {
            var recurrente = await _context.IngresosRecurrentes
                .Include(ir => ir.Categoria)
                .Include(ir => ir.Cuenta)
                .FirstOrDefaultAsync(ir => ir.Id == id && ir.UserId == userId);

            if (recurrente == null)
                return null;

            return new IngresoRecurrenteDto
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

        public async Task<(IngresoRecurrenteDto? result, string? error)> CreateIngresoRecurrenteAsync(string userId, CreateIngresoRecurrenteDto dto)
        {
            var categoria = await _context.Categorias.FindAsync(dto.CategoriaId);
            if (categoria == null || categoria.UserId != userId)
                return (null, "Categoria no valida");

            if (dto.CuentaId.HasValue)
            {
                var cuenta = await _context.Cuentas.FindAsync(dto.CuentaId.Value);
                if (cuenta == null || cuenta.UserId != userId)
                    return (null, "Cuenta no valida");
            }

            var proximaFecha = CalcularProximaFecha(dto.Frecuencia, dto.DiaDePago);

            var recurrente = new IngresoRecurrente
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

            _context.IngresosRecurrentes.Add(recurrente);
            await _context.SaveChangesAsync();

            await _context.Entry(recurrente).Reference(ir => ir.Categoria).LoadAsync();
            if (recurrente.CuentaId.HasValue)
                await _context.Entry(recurrente).Reference(ir => ir.Cuenta).LoadAsync();

            var result = new IngresoRecurrenteDto
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

        public async Task<(bool success, string? error)> UpdateIngresoRecurrenteAsync(string userId, int id, UpdateIngresoRecurrenteDto dto)
        {
            var recurrente = await _context.IngresosRecurrentes.FindAsync(id);
            if (recurrente == null || recurrente.UserId != userId)
                return (false, null);

            var categoria = await _context.Categorias.FindAsync(dto.CategoriaId);
            if (categoria == null || categoria.UserId != userId)
                return (false, "Categoria no valida");

            if (dto.CuentaId.HasValue)
            {
                var cuenta = await _context.Cuentas.FindAsync(dto.CuentaId.Value);
                if (cuenta == null || cuenta.UserId != userId)
                    return (false, "Cuenta no valida");
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

        public async Task<bool> DeleteIngresoRecurrenteAsync(string userId, int id)
        {
            var recurrente = await _context.IngresosRecurrentes.FindAsync(id);
            if (recurrente == null || recurrente.UserId != userId)
                return false;

            _context.IngresosRecurrentes.Remove(recurrente);
            await _context.SaveChangesAsync();

            return true;
        }

        public async Task<(Ingreso? result, string? error)> GenerarIngresoAsync(string userId, int id)
        {
            var recurrente = await _context.IngresosRecurrentes
                .Include(ir => ir.Cuenta)
                .FirstOrDefaultAsync(ir => ir.Id == id && ir.UserId == userId);

            if (recurrente == null)
                return (null, null);

            if (!recurrente.Activo)
                return (null, "El ingreso recurrente esta inactivo");

            var ingreso = new Ingreso
            {
                UserId = userId,
                CategoriaId = recurrente.CategoriaId,
                Descripcion = recurrente.Descripcion + " (Recurrente)",
                Monto = recurrente.Monto,
                CuentaId = recurrente.CuentaId,
                Fecha = DateTime.UtcNow
            };

            _context.Ingresos.Add(ingreso);

            if (recurrente.CuentaId.HasValue && recurrente.Cuenta != null)
            {
                recurrente.Cuenta.BalanceActual += recurrente.Monto;
            }

            recurrente.UltimaGeneracion = DateTime.UtcNow;
            recurrente.ProximaFecha = CalcularProximaFechaDesde(recurrente.ProximaFecha, recurrente.Frecuencia, recurrente.DiaDePago);

            await _context.SaveChangesAsync();

            return (ingreso, null);
        }

        public async Task<int> GenerarPendientesAsync(string userId)
        {
            var pendientes = await _context.IngresosRecurrentes
                .Include(ir => ir.Cuenta)
                .Where(ir => ir.UserId == userId && ir.Activo && ir.ProximaFecha <= DateTime.UtcNow)
                .ToListAsync();

            int generados = 0;

            foreach (var recurrente in pendientes)
            {
                var ingreso = new Ingreso
                {
                    UserId = userId,
                    CategoriaId = recurrente.CategoriaId,
                    Descripcion = recurrente.Descripcion + " (Recurrente)",
                    Monto = recurrente.Monto,
                    CuentaId = recurrente.CuentaId,
                    Fecha = recurrente.ProximaFecha
                };

                _context.Ingresos.Add(ingreso);

                if (recurrente.CuentaId.HasValue && recurrente.Cuenta != null)
                {
                    recurrente.Cuenta.BalanceActual += recurrente.Monto;
                }

                recurrente.UltimaGeneracion = DateTime.UtcNow;
                recurrente.ProximaFecha = CalcularProximaFechaDesde(recurrente.ProximaFecha, recurrente.Frecuencia, recurrente.DiaDePago);

                generados++;
            }

            await _context.SaveChangesAsync();

            return generados;
        }

        /// <summary>
        /// Calcula la proxima fecha de pago desde hoy.
        /// Para Quincenal con DiaDePago=15: paga el 15 y el ultimo dia del mes.
        /// </summary>
        private DateTime CalcularProximaFecha(string frecuencia, int dia)
        {
            var ahora = DateTime.UtcNow;

            if (frecuencia == "Semanal")
            {
                var diasHasta = ((dia - (int)ahora.DayOfWeek) + 7) % 7;
                return DateTime.SpecifyKind(ahora.Date.AddDays(diasHasta == 0 ? 7 : diasHasta), DateTimeKind.Utc);
            }

            if (frecuencia == "Quincenal")
            {
                var diaReal = Math.Min(dia, DateTime.DaysInMonth(ahora.Year, ahora.Month));
                var segundoDia = GetSegundoDiaQuincenal(dia, ahora.Year, ahora.Month);

                var fecha1 = new DateTime(ahora.Year, ahora.Month, diaReal, 0, 0, 0, DateTimeKind.Utc);
                var fecha2 = new DateTime(ahora.Year, ahora.Month, segundoDia, 0, 0, 0, DateTimeKind.Utc);

                // Ordenar las dos fechas
                if (fecha1 > fecha2) (fecha1, fecha2) = (fecha2, fecha1);

                if (ahora.Date < fecha1.Date) return fecha1;
                if (ahora.Date < fecha2.Date) return fecha2;

                // Ambas fechas ya pasaron, ir al primer dia del proximo mes
                var nextMonth = ahora.AddMonths(1);
                var diaNext = Math.Min(dia, DateTime.DaysInMonth(nextMonth.Year, nextMonth.Month));
                return new DateTime(nextMonth.Year, nextMonth.Month, diaNext, 0, 0, 0, DateTimeKind.Utc);
            }

            // Mensual o Anual
            var diaDelMes = Math.Min(dia, DateTime.DaysInMonth(ahora.Year, ahora.Month));
            var fechaBase = new DateTime(ahora.Year, ahora.Month, diaDelMes, 0, 0, 0, DateTimeKind.Utc);

            if (fechaBase <= ahora)
            {
                var next = frecuencia == "Anual" ? ahora.AddYears(1) : ahora.AddMonths(1);
                var d = Math.Min(dia, DateTime.DaysInMonth(next.Year, next.Month));
                fechaBase = new DateTime(next.Year, next.Month, d, 0, 0, 0, DateTimeKind.Utc);
            }

            return fechaBase;
        }

        /// <summary>
        /// Calcula la proxima fecha a partir de una fecha dada.
        /// Para Quincenal: alterna entre DiaDePago y el segundo dia (ej: 15 y 30/31).
        /// </summary>
        private DateTime CalcularProximaFechaDesde(DateTime desde, string frecuencia, int diaDePago)
        {
            switch (frecuencia)
            {
                case "Semanal":
                    return desde.AddDays(7);

                case "Quincenal":
                    var segundoDia = GetSegundoDiaQuincenal(diaDePago, desde.Year, desde.Month);
                    var diaReal = Math.Min(diaDePago, DateTime.DaysInMonth(desde.Year, desde.Month));

                    // Si estamos en el primer dia, ir al segundo dia del mismo mes
                    if (desde.Day == diaReal && diaReal != segundoDia)
                    {
                        return new DateTime(desde.Year, desde.Month, segundoDia, 0, 0, 0, DateTimeKind.Utc);
                    }

                    // Si estamos en el segundo dia (o cualquier otro caso), ir al primer dia del proximo mes
                    var nextMonth = desde.AddMonths(1);
                    var nextDia = Math.Min(diaDePago, DateTime.DaysInMonth(nextMonth.Year, nextMonth.Month));
                    return new DateTime(nextMonth.Year, nextMonth.Month, nextDia, 0, 0, 0, DateTimeKind.Utc);

                case "Anual":
                    return desde.AddYears(1);

                default: // Mensual
                    var nm = desde.AddMonths(1);
                    var d = Math.Min(desde.Day, DateTime.DaysInMonth(nm.Year, nm.Month));
                    return new DateTime(nm.Year, nm.Month, d, 0, 0, 0, DateTimeKind.Utc);
            }
        }

        /// <summary>
        /// Para Quincenal: si DiaDePago es menor o igual a 15, el segundo dia es el ultimo del mes.
        /// Si DiaDePago es mayor a 15, el segundo dia es DiaDePago - 15.
        /// </summary>
        private int GetSegundoDiaQuincenal(int diaDePago, int year, int month)
        {
            if (diaDePago <= 15)
                return DateTime.DaysInMonth(year, month);
            else
                return Math.Max(1, diaDePago - 15);
        }
    }
}
