using Microsoft.EntityFrameworkCore;
using FinanzasPersonales.Api.Data;
using FinanzasPersonales.Api.Dtos;

namespace FinanzasPersonales.Api.Services
{
    public class DashboardService : IDashboardService
    {
        private readonly FinanzasDbContext _context;

        public DashboardService(FinanzasDbContext context)
        {
            _context = context;
        }

        public async Task<DashboardDto> GetDashboardAsync(string userId, int? mes = null, int? ano = null)
        {
            var mesActual = mes ?? DateTime.Now.Month;
            var anoActual = ano ?? DateTime.Now.Year;

            // Resumen mes actual
            var ingresosMes = await _context.Ingresos
                .Where(i => i.UserId == userId && i.Fecha.Month == mesActual && i.Fecha.Year == anoActual)
                .SumAsync(i => (decimal?)i.Monto) ?? 0;

            var gastosMes = await _context.Gastos
                .Where(g => g.UserId == userId && g.Fecha.Month == mesActual && g.Fecha.Year == anoActual)
                .SumAsync(g => (decimal?)g.Monto) ?? 0;

            var cantTransacciones = await _context.Gastos
                .Where(g => g.UserId == userId && g.Fecha.Month == mesActual && g.Fecha.Year == anoActual)
                .CountAsync();

            var cantMetas = await _context.Metas
                .Where(m => m.UserId == userId)
                .CountAsync();

            var cantPresupuestos = await _context.Presupuestos
                .Where(p => p.UserId == userId && p.MesAplicable == mesActual && p.AnoAplicable == anoActual)
                .CountAsync();

            var resumenMes = new ResumenMesActualDto
            {
                Mes = mesActual,
                Ano = anoActual,
                TotalIngresos = ingresosMes,
                TotalGastos = gastosMes,
                Balance = ingresosMes - gastosMes,
                PromedioGastoDiario = DateTime.Now.Day > 0 ? gastosMes / DateTime.Now.Day : 0,
                CantidadTransacciones = cantTransacciones,
                MetasActivas = cantMetas,
                PresupuestosActivos = cantPresupuestos
            };

            // Últimos 6 meses - batch query en vez de loop N+1
            var fechaInicio6Meses = DateTime.SpecifyKind(
                new DateTime(DateTime.Now.AddMonths(-5).Year, DateTime.Now.AddMonths(-5).Month, 1), DateTimeKind.Utc);
            var fechaFin6Meses = DateTime.SpecifyKind(
                new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1).AddMonths(1).AddDays(-1), DateTimeKind.Utc);

            var ingresosPorMes = await _context.Ingresos
                .Where(i => i.UserId == userId && i.Fecha >= fechaInicio6Meses && i.Fecha <= fechaFin6Meses)
                .GroupBy(i => new { i.Fecha.Year, i.Fecha.Month })
                .Select(g => new { g.Key.Year, g.Key.Month, Total = g.Sum(i => i.Monto) })
                .ToListAsync();

            var gastosPorMes = await _context.Gastos
                .Where(g => g.UserId == userId && g.Fecha >= fechaInicio6Meses && g.Fecha <= fechaFin6Meses)
                .GroupBy(g => new { g.Fecha.Year, g.Fecha.Month })
                .Select(g => new { g.Key.Year, g.Key.Month, Total = g.Sum(x => x.Monto) })
                .ToListAsync();

            var ultimos6Meses = new List<EvolucionMensualDto>();
            for (int i = 5; i >= 0; i--)
            {
                var fecha = DateTime.Now.AddMonths(-i);
                var mesLoop = fecha.Month;
                var anoLoop = fecha.Year;

                var ingresos = ingresosPorMes.FirstOrDefault(x => x.Year == anoLoop && x.Month == mesLoop)?.Total ?? 0;
                var gastos = gastosPorMes.FirstOrDefault(x => x.Year == anoLoop && x.Month == mesLoop)?.Total ?? 0;

                ultimos6Meses.Add(new EvolucionMensualDto
                {
                    Mes = mesLoop,
                    Ano = anoLoop,
                    Periodo = $"{fecha:MMM yyyy}",
                    TotalIngresos = ingresos,
                    TotalGastos = gastos,
                    Balance = ingresos - gastos
                });
            }

            // Top categorias (mes actual)
            var topCategorias = await _context.Gastos
                .Where(g => g.UserId == userId && g.Fecha.Month == mesActual && g.Fecha.Year == anoActual)
                .GroupBy(g => new { g.CategoriaId, g.Categoria!.Nombre })
                .Select(group => new GastoPorCategoriaDto
                {
                    CategoriaId = group.Key.CategoriaId,
                    CategoriaNombre = group.Key.Nombre,
                    TotalGastado = group.Sum(g => g.Monto),
                    CantidadTransacciones = group.Count(),
                    PorcentajeDelTotal = 0
                })
                .OrderByDescending(g => g.TotalGastado)
                .Take(5)
                .ToListAsync();

            var totalGastos = topCategorias.Sum(c => c.TotalGastado);
            foreach (var cat in topCategorias)
            {
                cat.PorcentajeDelTotal = totalGastos > 0 ? (cat.TotalGastado / totalGastos) * 100 : 0;
            }

            // Presupuestos activos - batch query para gastado (evita N+1)
            var presupuestos = await _context.Presupuestos
                .Include(p => p.Categoria)
                .Where(p => p.UserId == userId && p.MesAplicable == mesActual && p.AnoAplicable == anoActual)
                .ToListAsync();

            var presCategIds = presupuestos.Select(p => p.CategoriaId).Distinct().ToList();
            var gastadoPorCategoria = await _context.Gastos
                .Where(g => g.UserId == userId &&
                           presCategIds.Contains(g.CategoriaId) &&
                           g.Fecha.Month == mesActual && g.Fecha.Year == anoActual)
                .GroupBy(g => g.CategoriaId)
                .Select(g => new { CategoriaId = g.Key, Total = g.Sum(x => x.Monto) })
                .ToDictionaryAsync(g => g.CategoriaId, g => g.Total);

            var presupuestosDto = presupuestos.Select(p =>
            {
                var gastado = gastadoPorCategoria.GetValueOrDefault(p.CategoriaId, 0);
                return new PresupuestoDto
                {
                    Id = p.Id,
                    CategoriaId = p.CategoriaId,
                    CategoriaNombre = p.Categoria?.Nombre ?? "",
                    MontoLimite = p.MontoLimite,
                    Periodo = p.Periodo,
                    MesAplicable = p.MesAplicable,
                    AnoAplicable = p.AnoAplicable,
                    GastadoActual = gastado,
                    Disponible = p.MontoLimite - gastado,
                    PorcentajeUtilizado = p.MontoLimite > 0 ? (gastado / p.MontoLimite) * 100 : 0
                };
            }).ToList();

            return new DashboardDto
            {
                MesActual = resumenMes,
                UltimosSeisMeses = ultimos6Meses,
                TopCategorias = topCategorias,
                PresupuestosActivos = presupuestosDto
            };
        }

        public async Task<GraficaDto> GetGraficaIngresosVsGastosAsync(string userId, int meses = 6)
        {
            if (meses > 12) meses = 12;
            if (meses < 1) meses = 1;

            // Batch query en vez de loop N+1
            var fechaInicio = DateTime.SpecifyKind(
                new DateTime(DateTime.Now.AddMonths(-(meses - 1)).Year, DateTime.Now.AddMonths(-(meses - 1)).Month, 1), DateTimeKind.Utc);
            var fechaFin = DateTime.SpecifyKind(
                new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1).AddMonths(1).AddDays(-1), DateTimeKind.Utc);

            var ingresosPorMes = await _context.Ingresos
                .Where(i => i.UserId == userId && i.Fecha >= fechaInicio && i.Fecha <= fechaFin)
                .GroupBy(i => new { i.Fecha.Year, i.Fecha.Month })
                .Select(g => new { g.Key.Year, g.Key.Month, Total = g.Sum(i => i.Monto) })
                .ToListAsync();

            var gastosPorMes = await _context.Gastos
                .Where(g => g.UserId == userId && g.Fecha >= fechaInicio && g.Fecha <= fechaFin)
                .GroupBy(g => new { g.Fecha.Year, g.Fecha.Month })
                .Select(g => new { g.Key.Year, g.Key.Month, Total = g.Sum(x => x.Monto) })
                .ToListAsync();

            var datos = new List<PuntoGraficaDto>();
            for (int i = meses - 1; i >= 0; i--)
            {
                var fecha = DateTime.Now.AddMonths(-i);
                var ingresos = ingresosPorMes.FirstOrDefault(x => x.Year == fecha.Year && x.Month == fecha.Month)?.Total ?? 0;
                var gastos = gastosPorMes.FirstOrDefault(x => x.Year == fecha.Year && x.Month == fecha.Month)?.Total ?? 0;

                datos.Add(new PuntoGraficaDto
                {
                    Etiqueta = $"{fecha:MMM yyyy} - Ingresos",
                    Valor = ingresos,
                    Color = "#4CAF50"
                });

                datos.Add(new PuntoGraficaDto
                {
                    Etiqueta = $"{fecha:MMM yyyy} - Gastos",
                    Valor = gastos,
                    Color = "#F44336"
                });
            }

            return new GraficaDto
            {
                Titulo = "Ingresos vs Gastos",
                Datos = datos
            };
        }

        public async Task<GraficaDto> GetGraficaGastosPorCategoriaAsync(string userId, int? mes = null, int? ano = null)
        {
            var mesConsulta = mes ?? DateTime.Now.Month;
            var anoConsulta = ano ?? DateTime.Now.Year;

            var gastosPorCategoria = await _context.Gastos
                .Where(g => g.UserId == userId && g.Fecha.Month == mesConsulta && g.Fecha.Year == anoConsulta)
                .GroupBy(g => new { g.CategoriaId, g.Categoria!.Nombre })
                .Select(group => new
                {
                    Nombre = group.Key.Nombre,
                    Total = group.Sum(g => g.Monto)
                })
                .OrderByDescending(x => x.Total)
                .ToListAsync();

            var colores = new[] { "#FF6384", "#36A2EB", "#FFCE56", "#4BC0C0", "#9966FF", "#FF9F40" };
            var datos = gastosPorCategoria.Select((cat, index) => new PuntoGraficaDto
            {
                Etiqueta = cat.Nombre,
                Valor = cat.Total,
                Color = colores[index % colores.Length]
            }).ToList();

            return new GraficaDto
            {
                Titulo = $"Gastos por Categoría - {new DateTime(anoConsulta, mesConsulta, 1):MMMM yyyy}",
                Datos = datos
            };
        }

        public async Task<GraficaDto> GetGraficaProgresoMetasAsync(string userId)
        {
            var metas = await _context.Metas
                .Where(m => m.UserId == userId)
                .ToListAsync();

            var datos = metas.Select(m => new PuntoGraficaDto
            {
                Etiqueta = m.Metas,
                Valor = m.MontoTotal > 0 ? (m.AhorroActual / m.MontoTotal) * 100 : 0,
                Color = m.AhorroActual >= m.MontoTotal ? "#4CAF50" : "#2196F3"
            }).ToList();

            return new GraficaDto
            {
                Titulo = "Progreso de Metas (%)",
                Datos = datos
            };
        }

        public async Task<DashboardMetricsDto> GetMetricsAsync(string userId)
        {
            var hoy = DateTime.UtcNow;
            var primerDiaMesActual = new DateTime(hoy.Year, hoy.Month, 1, 0, 0, 0, DateTimeKind.Utc);
            var ultimoDiaMesActual = primerDiaMesActual.AddMonths(1).AddDays(-1);
            var primerDiaMesAnterior = primerDiaMesActual.AddMonths(-1);
            var primerDia6MesesAtras = primerDiaMesActual.AddMonths(-5);

            // Totales mes actual (transacciones reales)
            var ingresosActual = await _context.Ingresos
                .Where(i => i.UserId == userId && i.Fecha >= primerDiaMesActual && i.Fecha <= ultimoDiaMesActual)
                .SumAsync(i => (decimal?)i.Monto) ?? 0;

            var gastosActual = await _context.Gastos
                .Where(g => g.UserId == userId && g.Fecha >= primerDiaMesActual && g.Fecha <= ultimoDiaMesActual)
                .SumAsync(g => (decimal?)g.Monto) ?? 0;

            // Incluir recurrentes pendientes del mes actual (aún no generados por el job)
            var ingresosRecurrentesPendientes = await _context.IngresosRecurrentes
                .Where(ir => ir.UserId == userId && ir.Activo
                    && ir.ProximaFecha >= primerDiaMesActual && ir.ProximaFecha <= ultimoDiaMesActual)
                .SumAsync(ir => (decimal?)ir.Monto) ?? 0;

            var gastosRecurrentesPendientes = await _context.GastosRecurrentes
                .Where(gr => gr.UserId == userId && gr.Activo
                    && gr.ProximaFecha >= primerDiaMesActual && gr.ProximaFecha <= ultimoDiaMesActual)
                .SumAsync(gr => (decimal?)gr.Monto) ?? 0;

            ingresosActual += ingresosRecurrentesPendientes;
            gastosActual += gastosRecurrentesPendientes;

            // Balance total de cuentas
            var balanceCuentas = await _context.Cuentas
                .Where(c => c.UserId == userId && c.Activa)
                .SumAsync(c => (decimal?)c.BalanceActual) ?? 0;

            // Totales mes anterior
            var gastosAnterior = await _context.Gastos
                .Where(g => g.UserId == userId && g.Fecha >= primerDiaMesAnterior && g.Fecha < primerDiaMesActual)
                .SumAsync(g => (decimal?)g.Monto) ?? 0;

            var cambio = gastosAnterior > 0 ? ((gastosActual - gastosAnterior) / gastosAnterior) * 100 : 0;

            // Tendencia 6 meses - batch query (evita N+1)
            var ingresosTendencia = await _context.Ingresos
                .Where(i => i.UserId == userId && i.Fecha >= primerDia6MesesAtras && i.Fecha <= ultimoDiaMesActual)
                .GroupBy(i => new { i.Fecha.Year, i.Fecha.Month })
                .Select(g => new { g.Key.Year, g.Key.Month, Total = g.Sum(i => i.Monto) })
                .ToListAsync();

            var gastosTendencia = await _context.Gastos
                .Where(g => g.UserId == userId && g.Fecha >= primerDia6MesesAtras && g.Fecha <= ultimoDiaMesActual)
                .GroupBy(g => new { g.Fecha.Year, g.Fecha.Month })
                .Select(g => new { g.Key.Year, g.Key.Month, Total = g.Sum(x => x.Monto) })
                .ToListAsync();

            var tendencia = new List<MesFinancieroDto>();
            for (int i = 5; i >= 0; i--)
            {
                var mesRef = hoy.AddMonths(-i);
                var ing = ingresosTendencia.FirstOrDefault(x => x.Year == mesRef.Year && x.Month == mesRef.Month)?.Total ?? 0;
                var gst = gastosTendencia.FirstOrDefault(x => x.Year == mesRef.Year && x.Month == mesRef.Month)?.Total ?? 0;

                tendencia.Add(new MesFinancieroDto
                {
                    Mes = mesRef.ToString("MMM yyyy"),
                    Ingresos = ing,
                    Gastos = gst
                });
            }

            // Top 5 categorias
            var top5 = await _context.Gastos
                .Where(g => g.UserId == userId && g.Fecha >= primerDiaMesActual && g.Fecha <= ultimoDiaMesActual && g.CategoriaId != null)
                .Include(g => g.Categoria)
                .GroupBy(g => new { g.CategoriaId, g.Categoria!.Nombre })
                .Select(g => new CategoriaTopDto
                {
                    Nombre = g.Key.Nombre,
                    Total = g.Sum(x => x.Monto),
                    Color = "#" + g.Key.CategoriaId.ToString()!.PadLeft(6, '0').Substring(0, 6)
                })
                .OrderByDescending(c => c.Total)
                .Take(5)
                .ToListAsync();

            return new DashboardMetricsDto
            {
                TotalIngresosDelMes = ingresosActual,
                TotalGastosDelMes = gastosActual,
                BalanceDelMes = ingresosActual - gastosActual,
                BalanceCuentas = balanceCuentas,
                CambioMesAnterior = cambio,
                Tendencia6Meses = tendencia,
                Top5Categorias = top5
            };
        }
        public async Task<FlujoCajaDto> GetFlujoCajaAsync(string userId)
        {
            var balanceTotal = await _context.Cuentas
                .Where(c => c.UserId == userId && c.Activa)
                .SumAsync(c => (decimal?)c.BalanceActual) ?? 0;

            var proyecciones = new List<ProyeccionFlujoCajaDto>();

            foreach (var dias in new[] { 30, 60, 90 })
            {
                var ahora = DateTime.UtcNow;
                var hasta = ahora.AddDays(dias);

                // Ingresos recurrentes esperados
                var ingresosRecurrentes = await _context.IngresosRecurrentes
                    .Where(ir => ir.UserId == userId && ir.Activo && ir.ProximaFecha <= hasta)
                    .ToListAsync();

                decimal ingresosEsperados = 0;
                foreach (var ir in ingresosRecurrentes)
                {
                    var fecha = ir.ProximaFecha;
                    while (fecha <= hasta)
                    {
                        ingresosEsperados += ir.Monto;
                        fecha = ir.Frecuencia switch
                        {
                            "Semanal" => fecha.AddDays(7),
                            "Quincenal" => fecha.AddDays(15),
                            "Mensual" => fecha.AddMonths(1),
                            "Anual" => fecha.AddYears(1),
                            _ => fecha.AddMonths(1)
                        };
                    }
                }

                // Gastos recurrentes esperados
                var gastosRecurrentes = await _context.GastosRecurrentes
                    .Where(gr => gr.UserId == userId && gr.Activo && gr.ProximaFecha <= hasta)
                    .ToListAsync();

                decimal gastosEsperados = 0;
                foreach (var gr in gastosRecurrentes)
                {
                    var fecha = gr.ProximaFecha;
                    while (fecha <= hasta)
                    {
                        gastosEsperados += gr.Monto;
                        fecha = gr.Frecuencia switch
                        {
                            "Semanal" => fecha.AddDays(7),
                            "Quincenal" => fecha.AddDays(15),
                            "Mensual" => fecha.AddMonths(1),
                            "Anual" => fecha.AddYears(1),
                            _ => fecha.AddMonths(1)
                        };
                    }
                }

                // Pagos de deuda esperados
                var deudasActivas = await _context.Deudas
                    .Where(d => d.UserId == userId && d.Activa && d.PagoMinimo.HasValue)
                    .ToListAsync();

                decimal pagosDeuda = 0;
                foreach (var deuda in deudasActivas)
                {
                    // Estimar cuántos pagos mensuales caben en el periodo
                    var mesesEnPeriodo = (int)Math.Ceiling(dias / 30.0);
                    pagosDeuda += (deuda.PagoMinimo ?? 0) * mesesEnPeriodo;
                }

                proyecciones.Add(new ProyeccionFlujoCajaDto
                {
                    Periodo = $"{dias} días",
                    Dias = dias,
                    FechaHasta = hasta,
                    IngresosEsperados = ingresosEsperados,
                    GastosEsperados = gastosEsperados,
                    PagosDeudaEsperados = pagosDeuda,
                    BalanceProyectado = balanceTotal + ingresosEsperados - gastosEsperados - pagosDeuda
                });
            }

            return new FlujoCajaDto
            {
                BalanceActualTotal = balanceTotal,
                Proyecciones = proyecciones
            };
        }
    }
}
