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

            // Ultimos 6 meses
            var ultimos6Meses = new List<EvolucionMensualDto>();
            for (int i = 5; i >= 0; i--)
            {
                var fecha = DateTime.Now.AddMonths(-i);
                var mesLoop = fecha.Month;
                var anoLoop = fecha.Year;

                var ingresos = await _context.Ingresos
                    .Where(ing => ing.UserId == userId && ing.Fecha.Month == mesLoop && ing.Fecha.Year == anoLoop)
                    .SumAsync(ing => (decimal?)ing.Monto) ?? 0;

                var gastos = await _context.Gastos
                    .Where(g => g.UserId == userId && g.Fecha.Month == mesLoop && g.Fecha.Year == anoLoop)
                    .SumAsync(g => (decimal?)g.Monto) ?? 0;

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

            // Presupuestos activos
            var presupuestos = await _context.Presupuestos
                .Include(p => p.Categoria)
                .Where(p => p.UserId == userId && p.MesAplicable == mesActual && p.AnoAplicable == anoActual)
                .ToListAsync();

            var presupuestosDto = new List<PresupuestoDto>();
            foreach (var p in presupuestos)
            {
                var gastado = await _context.Gastos
                    .Where(g => g.UserId == userId && g.CategoriaId == p.CategoriaId
                               && g.Fecha.Month == mesActual && g.Fecha.Year == anoActual)
                    .SumAsync(g => (decimal?)g.Monto) ?? 0;

                presupuestosDto.Add(new PresupuestoDto
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
                });
            }

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

            var datos = new List<PuntoGraficaDto>();

            for (int i = meses - 1; i >= 0; i--)
            {
                var fecha = DateTime.Now.AddMonths(-i);
                var mesVal = fecha.Month;
                var anoVal = fecha.Year;

                var ingresos = await _context.Ingresos
                    .Where(ing => ing.UserId == userId && ing.Fecha.Month == mesVal && ing.Fecha.Year == anoVal)
                    .SumAsync(ing => (decimal?)ing.Monto) ?? 0;

                var gastos = await _context.Gastos
                    .Where(g => g.UserId == userId && g.Fecha.Month == mesVal && g.Fecha.Year == anoVal)
                    .SumAsync(g => (decimal?)g.Monto) ?? 0;

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

            // Totales mes actual
            var ingresosActual = await _context.Ingresos
                .Where(i => i.UserId == userId && i.Fecha >= primerDiaMesActual && i.Fecha <= ultimoDiaMesActual)
                .SumAsync(i => (decimal?)i.Monto) ?? 0;

            var gastosActual = await _context.Gastos
                .Where(g => g.UserId == userId && g.Fecha >= primerDiaMesActual && g.Fecha <= ultimoDiaMesActual)
                .SumAsync(g => (decimal?)g.Monto) ?? 0;

            // Totales mes anterior
            var gastosAnterior = await _context.Gastos
                .Where(g => g.UserId == userId && g.Fecha >= primerDiaMesAnterior && g.Fecha < primerDiaMesActual)
                .SumAsync(g => (decimal?)g.Monto) ?? 0;

            var cambio = gastosAnterior > 0 ? ((gastosActual - gastosAnterior) / gastosAnterior) * 100 : 0;

            // Tendencia 6 meses
            var tendencia = new List<MesFinancieroDto>();
            for (int i = 5; i >= 0; i--)
            {
                var mesRef = hoy.AddMonths(-i);
                var primerDia = new DateTime(mesRef.Year, mesRef.Month, 1, 0, 0, 0, DateTimeKind.Utc);
                var ultimoDia = primerDia.AddMonths(1).AddDays(-1);

                var ing = await _context.Ingresos
                    .Where(x => x.UserId == userId && x.Fecha >= primerDia && x.Fecha <= ultimoDia)
                    .SumAsync(x => (decimal?)x.Monto) ?? 0;

                var gst = await _context.Gastos
                    .Where(x => x.UserId == userId && x.Fecha >= primerDia && x.Fecha <= ultimoDia)
                    .SumAsync(x => (decimal?)x.Monto) ?? 0;

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
                CambioMesAnterior = cambio,
                Tendencia6Meses = tendencia,
                Top5Categorias = top5
            };
        }
    }
}
