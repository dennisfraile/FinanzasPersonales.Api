using Microsoft.EntityFrameworkCore;
using FinanzasPersonales.Api.Data;
using FinanzasPersonales.Api.Dtos;
using System.Globalization;

namespace FinanzasPersonales.Api.Services
{
    public class CuentaDashboardService : ICuentaDashboardService
    {
        private readonly FinanzasDbContext _context;
        private readonly IMetasService _metasService;

        public CuentaDashboardService(FinanzasDbContext context, IMetasService metasService)
        {
            _context = context;
            _metasService = metasService;
        }

        public async Task<CuentaDashboardDto?> GetCuentaDashboardAsync(string userId, int cuentaId, int page = 1, int pageSize = 50)
        {
            var cuenta = await _context.Cuentas
                .FirstOrDefaultAsync(c => c.Id == cuentaId && c.UserId == userId && c.Activa);

            if (cuenta == null)
                return null;

            // 1. Obtener todas las transacciones de la cuenta
            var transacciones = await ObtenerTransaccionesTimelineAsync(userId, cuentaId, cuenta.BalanceInicial);

            var totalTransacciones = transacciones.Count;

            // Paginar (más recientes primero)
            var transaccionesPaginadas = transacciones
                .OrderByDescending(t => t.Fecha)
                .ThenByDescending(t => t.Id)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            // 2. Próximos recurrentes para esta cuenta
            var limite = DateTime.UtcNow.AddDays(30);
            var proximosIngresos = await _context.IngresosRecurrentes
                .Where(ir => ir.UserId == userId && ir.CuentaId == cuentaId && ir.Activo && ir.ProximaFecha <= limite)
                .Select(ir => new RecurrenteProximoDto
                {
                    Id = ir.Id,
                    Tipo = "Ingreso",
                    Descripcion = ir.Descripcion,
                    Monto = ir.Monto,
                    ProximaFecha = ir.ProximaFecha,
                    Frecuencia = ir.Frecuencia
                })
                .ToListAsync();

            var proximosGastos = await _context.GastosRecurrentes
                .Where(gr => gr.UserId == userId && gr.CuentaId == cuentaId && gr.Activo && gr.ProximaFecha <= limite)
                .Select(gr => new RecurrenteProximoDto
                {
                    Id = gr.Id,
                    Tipo = "Gasto",
                    Descripcion = gr.Descripcion,
                    Monto = gr.Monto,
                    ProximaFecha = gr.ProximaFecha,
                    Frecuencia = gr.Frecuencia
                })
                .ToListAsync();

            var proximos = proximosIngresos.Concat(proximosGastos)
                .OrderBy(p => p.ProximaFecha)
                .ToList();

            // 3. Resumen mensual (últimos 6 meses)
            var resumenMensual = await ObtenerResumenMensualAsync(userId, cuentaId);

            // 4. Surplus de la quincena actual
            var surplus = await CalcularSurplusQuincenaAsync(userId, cuentaId);

            return new CuentaDashboardDto
            {
                CuentaId = cuenta.Id,
                Nombre = cuenta.Nombre,
                Tipo = cuenta.Tipo.ToString(),
                BalanceActual = cuenta.BalanceActual,
                BalanceInicial = cuenta.BalanceInicial,
                Moneda = cuenta.Moneda,
                Color = cuenta.Color,
                Transacciones = transaccionesPaginadas,
                TotalTransacciones = totalTransacciones,
                Proximos = proximos,
                ResumenMensual = resumenMensual,
                SurplusActual = surplus
            };
        }

        public async Task<(bool success, string? error)> AsignarSurplusAsync(string userId, AsignarSurplusDto dto)
        {
            if (dto.Monto <= 0)
                return (false, "El monto debe ser mayor a 0");

            var cuenta = await _context.Cuentas
                .FirstOrDefaultAsync(c => c.Id == dto.CuentaId && c.UserId == userId && c.Activa);

            if (cuenta == null)
                return (false, "Cuenta no encontrada");

            // Verificar que hay surplus disponible
            var surplus = await CalcularSurplusQuincenaAsync(userId, dto.CuentaId);
            if (surplus == null || surplus.Surplus <= 0)
                return (false, "No hay surplus disponible en la quincena actual");

            if (dto.Monto > surplus.Surplus)
                return (false, $"El monto excede el surplus disponible ({surplus.Surplus:F2})");

            if (dto.Destino == "BalanceInicial")
            {
                cuenta.BalanceInicial += dto.Monto;
                await _context.SaveChangesAsync();
                return (true, null);
            }

            if (dto.Destino == "Meta")
            {
                if (!dto.MetaId.HasValue)
                    return (false, "Debe especificar una meta");

                var resultado = await _metasService.AbonarMetaAsync(dto.MetaId.Value, userId, dto.Monto);
                if (!resultado)
                    return (false, "Meta no encontrada o no pertenece al usuario");

                return (true, null);
            }

            return (false, "Destino no válido. Use 'BalanceInicial' o 'Meta'");
        }

        private async Task<List<TransaccionTimelineDto>> ObtenerTransaccionesTimelineAsync(string userId, int cuentaId, decimal balanceInicial)
        {
            // Ingresos
            var ingresos = await _context.Ingresos
                .Where(i => i.UserId == userId && i.CuentaId == cuentaId)
                .Include(i => i.Categoria)
                .Select(i => new TransaccionTimelineDto
                {
                    Id = i.Id,
                    Fecha = i.Fecha,
                    Tipo = "Ingreso",
                    Descripcion = i.Descripcion,
                    Categoria = i.Categoria != null ? i.Categoria.Nombre : null,
                    Monto = i.Monto,
                    EsRecurrente = i.Descripcion.Contains("(Recurrente)")
                })
                .ToListAsync();

            // Gastos
            var gastos = await _context.Gastos
                .Where(g => g.UserId == userId && g.CuentaId == cuentaId)
                .Include(g => g.Categoria)
                .Select(g => new TransaccionTimelineDto
                {
                    Id = g.Id,
                    Fecha = g.Fecha,
                    Tipo = "Gasto",
                    Descripcion = g.Descripcion,
                    Categoria = g.Categoria != null ? g.Categoria.Nombre : null,
                    Monto = g.Monto,
                    EsRecurrente = g.Descripcion.Contains("(Recurrente)")
                })
                .ToListAsync();

            // Transferencias de entrada
            var transferenciasEntrada = await _context.Set<Models.Transferencia>()
                .Where(t => t.UserId == userId && t.CuentaDestinoId == cuentaId)
                .Include(t => t.CuentaOrigen)
                .Select(t => new TransaccionTimelineDto
                {
                    Id = t.Id,
                    Fecha = t.Fecha,
                    Tipo = "TransferenciaEntrada",
                    Descripcion = t.Descripcion ?? $"Transferencia desde {(t.CuentaOrigen != null ? t.CuentaOrigen.Nombre : "otra cuenta")}",
                    Monto = t.Monto,
                    EsRecurrente = false
                })
                .ToListAsync();

            // Transferencias de salida
            var transferenciasSalida = await _context.Set<Models.Transferencia>()
                .Where(t => t.UserId == userId && t.CuentaOrigenId == cuentaId)
                .Include(t => t.CuentaDestino)
                .Select(t => new TransaccionTimelineDto
                {
                    Id = t.Id,
                    Fecha = t.Fecha,
                    Tipo = "TransferenciaSalida",
                    Descripcion = t.Descripcion ?? $"Transferencia a {(t.CuentaDestino != null ? t.CuentaDestino.Nombre : "otra cuenta")}",
                    Monto = t.Monto,
                    EsRecurrente = false
                })
                .ToListAsync();

            // Unir y ordenar por fecha ascendente para calcular balance corriente
            var todas = ingresos
                .Concat(gastos)
                .Concat(transferenciasEntrada)
                .Concat(transferenciasSalida)
                .OrderBy(t => t.Fecha)
                .ThenBy(t => t.Id)
                .ToList();

            // Calcular saldo corriente (running balance)
            decimal balance = balanceInicial;
            foreach (var t in todas)
            {
                if (t.Tipo == "Ingreso" || t.Tipo == "TransferenciaEntrada")
                    balance += t.Monto;
                else
                    balance -= t.Monto;

                t.BalanceDespues = balance;
            }

            return todas;
        }

        private async Task<List<ResumenMensualCuentaDto>> ObtenerResumenMensualAsync(string userId, int cuentaId)
        {
            var hoy = DateTime.UtcNow;
            var fechaInicio = new DateTime(hoy.AddMonths(-5).Year, hoy.AddMonths(-5).Month, 1, 0, 0, 0, DateTimeKind.Utc);
            var fechaFin = new DateTime(hoy.Year, hoy.Month, 1, 0, 0, 0, DateTimeKind.Utc).AddMonths(1).AddDays(-1);

            var ingresosPorMes = await _context.Ingresos
                .Where(i => i.UserId == userId && i.CuentaId == cuentaId && i.Fecha >= fechaInicio && i.Fecha <= fechaFin)
                .GroupBy(i => new { i.Fecha.Year, i.Fecha.Month })
                .Select(g => new { g.Key.Year, g.Key.Month, Total = g.Sum(i => i.Monto) })
                .ToListAsync();

            var gastosPorMes = await _context.Gastos
                .Where(g => g.UserId == userId && g.CuentaId == cuentaId && g.Fecha >= fechaInicio && g.Fecha <= fechaFin)
                .GroupBy(g => new { g.Fecha.Year, g.Fecha.Month })
                .Select(g => new { g.Key.Year, g.Key.Month, Total = g.Sum(x => x.Monto) })
                .ToListAsync();

            var resultado = new List<ResumenMensualCuentaDto>();
            for (int i = 5; i >= 0; i--)
            {
                var mesRef = hoy.AddMonths(-i);
                var ing = ingresosPorMes.FirstOrDefault(x => x.Year == mesRef.Year && x.Month == mesRef.Month)?.Total ?? 0;
                var gas = gastosPorMes.FirstOrDefault(x => x.Year == mesRef.Year && x.Month == mesRef.Month)?.Total ?? 0;

                resultado.Add(new ResumenMensualCuentaDto
                {
                    Mes = mesRef.Month,
                    Ano = mesRef.Year,
                    Periodo = mesRef.ToString("MMM yyyy", new CultureInfo("es-ES")),
                    TotalIngresos = ing,
                    TotalGastos = gas,
                    Balance = ing - gas
                });
            }

            return resultado;
        }

        private async Task<SurplusQuincenaDto> CalcularSurplusQuincenaAsync(string userId, int cuentaId)
        {
            var hoy = DateTime.UtcNow;
            var (inicio, fin, nombre) = GetQuincenaActual(hoy);

            var ingresos = await _context.Ingresos
                .Where(i => i.UserId == userId && i.CuentaId == cuentaId && i.Fecha >= inicio && i.Fecha <= fin)
                .SumAsync(i => (decimal?)i.Monto) ?? 0;

            var gastos = await _context.Gastos
                .Where(g => g.UserId == userId && g.CuentaId == cuentaId && g.Fecha >= inicio && g.Fecha <= fin)
                .SumAsync(g => (decimal?)g.Monto) ?? 0;

            // Incluir recurrentes pendientes de esta quincena para esta cuenta
            var ingRecurrentes = await _context.IngresosRecurrentes
                .Where(ir => ir.UserId == userId && ir.CuentaId == cuentaId && ir.Activo
                    && ir.ProximaFecha >= inicio && ir.ProximaFecha <= fin)
                .SumAsync(ir => (decimal?)ir.Monto) ?? 0;

            var gasRecurrentes = await _context.GastosRecurrentes
                .Where(gr => gr.UserId == userId && gr.CuentaId == cuentaId && gr.Activo
                    && gr.ProximaFecha >= inicio && gr.ProximaFecha <= fin)
                .SumAsync(gr => (decimal?)gr.Monto) ?? 0;

            ingresos += ingRecurrentes;
            gastos += gasRecurrentes;

            var periodoTerminado = hoy.Day > 15
                ? hoy.Day >= DateTime.DaysInMonth(hoy.Year, hoy.Month)
                : hoy.Day >= 15;

            return new SurplusQuincenaDto
            {
                Periodo = nombre,
                FechaInicio = inicio,
                FechaFin = fin,
                TotalIngresos = ingresos,
                TotalGastos = gastos,
                Surplus = Math.Max(0, ingresos - gastos),
                PeriodoTerminado = periodoTerminado
            };
        }

        private static (DateTime inicio, DateTime fin, string nombre) GetQuincenaActual(DateTime fecha)
        {
            if (fecha.Day <= 15)
            {
                return (
                    new DateTime(fecha.Year, fecha.Month, 1, 0, 0, 0, DateTimeKind.Utc),
                    new DateTime(fecha.Year, fecha.Month, 15, 23, 59, 59, DateTimeKind.Utc),
                    $"Q1 {fecha.ToString("MMMM yyyy", new CultureInfo("es-ES"))}"
                );
            }
            else
            {
                return (
                    new DateTime(fecha.Year, fecha.Month, 16, 0, 0, 0, DateTimeKind.Utc),
                    new DateTime(fecha.Year, fecha.Month, DateTime.DaysInMonth(fecha.Year, fecha.Month), 23, 59, 59, DateTimeKind.Utc),
                    $"Q2 {fecha.ToString("MMMM yyyy", new CultureInfo("es-ES"))}"
                );
            }
        }
    }
}
