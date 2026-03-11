using Microsoft.EntityFrameworkCore;
using FinanzasPersonales.Api.Data;
using FinanzasPersonales.Api.Dtos;
using System.Globalization;

namespace FinanzasPersonales.Api.Services
{
    public class ReportesService : IReportesService
    {
        private readonly FinanzasDbContext _context;

        public ReportesService(FinanzasDbContext context)
        {
            _context = context;
        }

        public async Task<List<GastoPorCategoriaDto>> GetGastosPorCategoriaAsync(string userId, int? mes = null, int? ano = null)
        {
            var mesActual = mes ?? DateTime.Now.Month;
            var anoActual = ano ?? DateTime.Now.Year;

            var inicioMes = new DateTime(anoActual, mesActual, 1);
            var finMes = inicioMes.AddMonths(1).AddDays(-1);

            var gastosPorCategoria = await _context.Gastos
                .Where(g => g.UserId == userId &&
                           g.Fecha >= inicioMes &&
                           g.Fecha <= finMes)
                .GroupBy(g => new { g.CategoriaId, g.Categoria.Nombre })
                .Select(grupo => new
                {
                    CategoriaId = grupo.Key.CategoriaId,
                    CategoriaNombre = grupo.Key.Nombre,
                    TotalGastado = grupo.Sum(g => g.Monto),
                    CantidadTransacciones = grupo.Count()
                })
                .OrderByDescending(x => x.TotalGastado)
                .ToListAsync();

            var totalGeneral = gastosPorCategoria.Sum(x => x.TotalGastado);

            return gastosPorCategoria.Select(x => new GastoPorCategoriaDto
            {
                CategoriaId = x.CategoriaId,
                CategoriaNombre = x.CategoriaNombre,
                TotalGastado = x.TotalGastado,
                CantidadTransacciones = x.CantidadTransacciones,
                PorcentajeDelTotal = totalGeneral > 0 ? (x.TotalGastado / totalGeneral) * 100 : 0
            }).ToList();
        }

        public async Task<List<EvolucionMensualDto>> GetEvolucionMensualAsync(string userId, int meses = 6)
        {
            meses = Math.Min(meses, 12);
            var fechaActual = DateTime.Now;
            var fechaInicio = new DateTime(fechaActual.AddMonths(-(meses - 1)).Year, fechaActual.AddMonths(-(meses - 1)).Month, 1);
            var fechaFin = new DateTime(fechaActual.Year, fechaActual.Month, 1).AddMonths(1).AddDays(-1);

            // Batch: 2 queries en vez de 2*meses
            var ingresosPorMes = await _context.Ingresos
                .Where(x => x.UserId == userId && x.Fecha >= fechaInicio && x.Fecha <= fechaFin)
                .GroupBy(x => new { x.Fecha.Year, x.Fecha.Month })
                .Select(g => new { g.Key.Year, g.Key.Month, Total = g.Sum(x => x.Monto) })
                .ToListAsync();

            var gastosPorMes = await _context.Gastos
                .Where(x => x.UserId == userId && x.Fecha >= fechaInicio && x.Fecha <= fechaFin)
                .GroupBy(x => new { x.Fecha.Year, x.Fecha.Month })
                .Select(g => new { g.Key.Year, g.Key.Month, Total = g.Sum(x => x.Monto) })
                .ToListAsync();

            var resultado = new List<EvolucionMensualDto>();
            for (int i = meses - 1; i >= 0; i--)
            {
                var mesRef = fechaActual.AddMonths(-i);
                var ingresos = ingresosPorMes.FirstOrDefault(x => x.Year == mesRef.Year && x.Month == mesRef.Month)?.Total ?? 0;
                var gastos = gastosPorMes.FirstOrDefault(x => x.Year == mesRef.Year && x.Month == mesRef.Month)?.Total ?? 0;

                resultado.Add(new EvolucionMensualDto
                {
                    Mes = mesRef.Month,
                    Ano = mesRef.Year,
                    Periodo = mesRef.ToString("MMMM yyyy", new CultureInfo("es-ES")),
                    TotalIngresos = ingresos,
                    TotalGastos = gastos,
                    AhorroCalculado = ingresos * 0.10m,
                    Balance = ingresos - gastos
                });
            }

            return resultado;
        }

        public async Task<ComparativaPeriodosDto> GetComparativaPeriodosAsync(string userId, int? mesActual = null, int? anoActual = null, int? mesAnterior = null, int? anoAnterior = null)
        {
            var fechaActual = DateTime.Now;
            var mesA = mesActual ?? fechaActual.Month;
            var anoA = anoActual ?? fechaActual.Year;

            var fechaAnt = fechaActual.AddMonths(-1);
            var mesAnt = mesAnterior ?? fechaAnt.Month;
            var anoAnt = anoAnterior ?? fechaAnt.Year;

            // Periodo actual
            var inicioMA = new DateTime(anoA, mesA, 1);
            var finMA = inicioMA.AddMonths(1).AddDays(-1);

            var ingresosActualVal = await _context.Ingresos
                .Where(x => x.UserId == userId && x.Fecha >= inicioMA && x.Fecha <= finMA)
                .SumAsync(x => x.Monto);

            var gastosActualVal = await _context.Gastos
                .Where(x => x.UserId == userId && x.Fecha >= inicioMA && x.Fecha <= finMA)
                .SumAsync(x => x.Monto);

            // Periodo anterior
            var inicioMAnt = new DateTime(anoAnt, mesAnt, 1);
            var finMAnt = inicioMAnt.AddMonths(1).AddDays(-1);

            var ingresosAnteriorVal = await _context.Ingresos
                .Where(x => x.UserId == userId && x.Fecha >= inicioMAnt && x.Fecha <= finMAnt)
                .SumAsync(x => x.Monto);

            var gastosAnteriorVal = await _context.Gastos
                .Where(x => x.UserId == userId && x.Fecha >= inicioMAnt && x.Fecha <= finMAnt)
                .SumAsync(x => x.Monto);

            return new ComparativaPeriodosDto
            {
                PeriodoActual = new PeriodoFinanciero
                {
                    Descripcion = new DateTime(anoA, mesA, 1).ToString("MMMM yyyy", new CultureInfo("es-ES")),
                    TotalIngresos = ingresosActualVal,
                    TotalGastos = gastosActualVal,
                    Balance = ingresosActualVal - gastosActualVal
                },
                PeriodoAnterior = new PeriodoFinanciero
                {
                    Descripcion = new DateTime(anoAnt, mesAnt, 1).ToString("MMMM yyyy", new CultureInfo("es-ES")),
                    TotalIngresos = ingresosAnteriorVal,
                    TotalGastos = gastosAnteriorVal,
                    Balance = ingresosAnteriorVal - gastosAnteriorVal
                },
                DiferenciaIngresos = ingresosActualVal - ingresosAnteriorVal,
                DiferenciaGastos = gastosActualVal - gastosAnteriorVal,
                DiferenciaBalance = (ingresosActualVal - gastosActualVal) - (ingresosAnteriorVal - gastosAnteriorVal),
                PorcentajeCambioIngresos = ingresosAnteriorVal != 0
                    ? ((ingresosActualVal - ingresosAnteriorVal) / ingresosAnteriorVal) * 100
                    : 0,
                PorcentajeCambioGastos = gastosAnteriorVal != 0
                    ? ((gastosActualVal - gastosAnteriorVal) / gastosAnteriorVal) * 100
                    : 0
            };
        }

        public async Task<ResumenGeneralDto> GetResumenGeneralAsync(string userId, DateTime? desde = null, DateTime? hasta = null)
        {
            var fechaDesde = desde ?? new DateTime(DateTime.Now.Year, 1, 1);
            var fechaHasta = hasta ?? DateTime.Now;

            var ingresos = await _context.Ingresos
                .Where(x => x.UserId == userId && x.Fecha >= fechaDesde && x.Fecha <= fechaHasta)
                .ToListAsync();

            var gastos = await _context.Gastos
                .Where(x => x.UserId == userId && x.Fecha >= fechaDesde && x.Fecha <= fechaHasta)
                .Include(x => x.Categoria)
                .ToListAsync();

            var totalIngresos = ingresos.Sum(x => x.Monto);
            var totalGastos = gastos.Sum(x => x.Monto);
            var diasConActividad = ingresos.Select(x => x.Fecha.Date)
                .Union(gastos.Select(x => x.Fecha.Date))
                .Distinct()
                .Count();

            var categoriaConMasGasto = gastos
                .GroupBy(x => x.Categoria.Nombre)
                .Select(g => new { Categoria = g.Key, Monto = g.Sum(x => x.Monto) })
                .OrderByDescending(x => x.Monto)
                .FirstOrDefault();

            var totalDias = (fechaHasta - fechaDesde).Days + 1;
            var totalMeses = ((fechaHasta.Year - fechaDesde.Year) * 12) + fechaHasta.Month - fechaDesde.Month + 1;

            return new ResumenGeneralDto
            {
                PeriodoAnalizado = $"{fechaDesde:dd/MM/yyyy} - {fechaHasta:dd/MM/yyyy}",
                TotalIngresos = totalIngresos,
                TotalGastos = totalGastos,
                Balance = totalIngresos - totalGastos,
                PromedioIngresosDiario = totalDias > 0 ? totalIngresos / totalDias : 0,
                PromedioGastosDiario = totalDias > 0 ? totalGastos / totalDias : 0,
                PromedioIngresosMensual = totalMeses > 0 ? totalIngresos / totalMeses : 0,
                PromedioGastosMensual = totalMeses > 0 ? totalGastos / totalMeses : 0,
                DiasConActividad = diasConActividad,
                CategoriaConMasGasto = categoriaConMasGasto?.Categoria ?? "N/A",
                MontoCategoriaMasGasto = categoriaConMasGasto?.Monto ?? 0
            };
        }

        public async Task<TendenciasMensualesDto> GetTendenciasAsync(string userId, int meses = 6)
        {
            meses = Math.Min(meses, 12);

            var fechaActual = DateTime.Now;
            var fechaInicio = fechaActual.AddMonths(-(meses - 1));
            var inicioMes = new DateTime(fechaInicio.Year, fechaInicio.Month, 1);
            var finMes = new DateTime(fechaActual.Year, fechaActual.Month, 1).AddMonths(1).AddDays(-1);

            // Batch: 2 queries en vez de 2*meses
            var ingresosPorMes = await _context.Ingresos
                .Where(x => x.UserId == userId && x.Fecha >= inicioMes && x.Fecha <= finMes)
                .GroupBy(x => new { x.Fecha.Year, x.Fecha.Month })
                .Select(g => new { g.Key.Year, g.Key.Month, Total = g.Sum(x => x.Monto) })
                .ToListAsync();

            var gastosPorMes = await _context.Gastos
                .Where(x => x.UserId == userId && x.Fecha >= inicioMes && x.Fecha <= finMes)
                .GroupBy(x => new { x.Fecha.Year, x.Fecha.Month })
                .Select(g => new { g.Key.Year, g.Key.Month, Total = g.Sum(x => x.Monto) })
                .ToListAsync();

            var datos = new List<DatoMensualDto>();
            for (int i = 0; i < meses; i++)
            {
                var mesRef = inicioMes.AddMonths(i);
                var ingresos = ingresosPorMes.FirstOrDefault(x => x.Year == mesRef.Year && x.Month == mesRef.Month)?.Total ?? 0;
                var gastos = gastosPorMes.FirstOrDefault(x => x.Year == mesRef.Year && x.Month == mesRef.Month)?.Total ?? 0;

                datos.Add(new DatoMensualDto
                {
                    Mes = mesRef.Month,
                    Ano = mesRef.Year,
                    Periodo = mesRef.ToString("MMM yyyy", new CultureInfo("es-ES")),
                    TotalIngresos = ingresos,
                    TotalGastos = gastos,
                    Balance = ingresos - gastos
                });
            }

            return new TendenciasMensualesDto
            {
                Periodo = new PeriodoDto
                {
                    Inicio = inicioMes,
                    Fin = fechaActual
                },
                Datos = datos
            };
        }

        public async Task<ComparativaMesDto> GetComparativaAsync(string userId, int? mes = null, int? ano = null)
        {
            var mesActual = mes ?? DateTime.Now.Month;
            var anoActual = ano ?? DateTime.Now.Year;

            // Mes actual
            var inicioActual = new DateTime(anoActual, mesActual, 1);
            var finActual = inicioActual.AddMonths(1).AddDays(-1);

            // Mes anterior
            var mesAnt = inicioActual.AddMonths(-1);
            var inicioAnterior = new DateTime(mesAnt.Year, mesAnt.Month, 1);
            var finAnterior = inicioAnterior.AddMonths(1).AddDays(-1);

            // Datos mes actual
            var ingresosActualVal = await _context.Ingresos
                .Where(x => x.UserId == userId && x.Fecha >= inicioActual && x.Fecha <= finActual)
                .SumAsync(x => x.Monto);

            var gastosActualVal = await _context.Gastos
                .Where(x => x.UserId == userId && x.Fecha >= inicioActual && x.Fecha <= finActual)
                .SumAsync(x => x.Monto);

            // Datos mes anterior
            var ingresosAnteriorVal = await _context.Ingresos
                .Where(x => x.UserId == userId && x.Fecha >= inicioAnterior && x.Fecha <= finAnterior)
                .SumAsync(x => x.Monto);

            var gastosAnteriorVal = await _context.Gastos
                .Where(x => x.UserId == userId && x.Fecha >= inicioAnterior && x.Fecha <= finAnterior)
                .SumAsync(x => x.Monto);

            // Comparativa por categorias
            var categoriasActual = await _context.Gastos
                .Where(x => x.UserId == userId && x.Fecha >= inicioActual && x.Fecha <= finActual)
                .GroupBy(x => new { x.CategoriaId, x.Categoria.Nombre })
                .Select(g => new { CategoriaId = g.Key.CategoriaId, Nombre = g.Key.Nombre, Total = g.Sum(x => x.Monto) })
                .ToListAsync();

            var categoriasAnterior = await _context.Gastos
                .Where(x => x.UserId == userId && x.Fecha >= inicioAnterior && x.Fecha <= finAnterior)
                .GroupBy(x => new { x.CategoriaId, x.Categoria.Nombre })
                .Select(g => new { CategoriaId = g.Key.CategoriaId, Nombre = g.Key.Nombre, Total = g.Sum(x => x.Monto) })
                .ToDictionaryAsync(x => x.CategoriaId, x => x.Total);

            var comparativaCategorias = categoriasActual.Select(ca => new ComparativaCategoriaDto
            {
                CategoriaId = ca.CategoriaId,
                Nombre = ca.Nombre,
                MesActual = ca.Total,
                MesAnterior = categoriasAnterior.GetValueOrDefault(ca.CategoriaId, 0),
                Cambio = categoriasAnterior.GetValueOrDefault(ca.CategoriaId, 0) > 0
                    ? ((ca.Total - categoriasAnterior[ca.CategoriaId]) / categoriasAnterior[ca.CategoriaId]) * 100
                    : 0
            }).ToList();

            var balanceActual = ingresosActualVal - gastosActualVal;
            var balanceAnterior = ingresosAnteriorVal - gastosAnteriorVal;

            return new ComparativaMesDto
            {
                MesActual = new ResumenMesDto
                {
                    Mes = mesActual,
                    Ano = anoActual,
                    TotalIngresos = ingresosActualVal,
                    TotalGastos = gastosActualVal,
                    Balance = balanceActual
                },
                MesAnterior = new ResumenMesDto
                {
                    Mes = mesAnt.Month,
                    Ano = mesAnt.Year,
                    TotalIngresos = ingresosAnteriorVal,
                    TotalGastos = gastosAnteriorVal,
                    Balance = balanceAnterior
                },
                Cambios = new CambiosDto
                {
                    IngresosPorcentaje = ingresosAnteriorVal > 0 ? ((ingresosActualVal - ingresosAnteriorVal) / ingresosAnteriorVal) * 100 : 0,
                    GastosPorcentaje = gastosAnteriorVal > 0 ? ((gastosActualVal - gastosAnteriorVal) / gastosAnteriorVal) * 100 : 0,
                    BalancePorcentaje = balanceAnterior > 0 ? ((balanceActual - balanceAnterior) / balanceAnterior) * 100 : 0
                },
                Categorias = comparativaCategorias
            };
        }

        public async Task<TopCategoriasDto> GetTopCategoriasAsync(string userId, int? mes = null, int? ano = null, int limite = 5)
        {
            var mesActual = mes ?? DateTime.Now.Month;
            var anoActual = ano ?? DateTime.Now.Year;

            var inicioMes = new DateTime(anoActual, mesActual, 1);
            var finMes = inicioMes.AddMonths(1).AddDays(-1);

            var gastosPorCategoria = await _context.Gastos
                .Where(x => x.UserId == userId && x.Fecha >= inicioMes && x.Fecha <= finMes)
                .GroupBy(x => new { x.CategoriaId, x.Categoria.Nombre })
                .Select(g => new
                {
                    CategoriaId = g.Key.CategoriaId,
                    Nombre = g.Key.Nombre,
                    Total = g.Sum(x => x.Monto),
                    Cantidad = g.Count()
                })
                .OrderByDescending(x => x.Total)
                .Take(limite)
                .ToListAsync();

            var totalGastos = await _context.Gastos
                .Where(x => x.UserId == userId && x.Fecha >= inicioMes && x.Fecha <= finMes)
                .SumAsync(x => x.Monto);

            return new TopCategoriasDto
            {
                Mes = mesActual,
                Ano = anoActual,
                TotalGastos = totalGastos,
                Categorias = gastosPorCategoria.Select(x => new CategoriaGastoDto
                {
                    CategoriaId = x.CategoriaId,
                    Nombre = x.Nombre,
                    Total = x.Total,
                    Porcentaje = totalGastos > 0 ? (x.Total / totalGastos) * 100 : 0,
                    CantidadTransacciones = x.Cantidad
                }).ToList()
            };
        }

        public async Task<GastosTipoDto> GetGastosTipoAsync(string userId, int? mes = null, int? ano = null)
        {
            var mesActual = mes ?? DateTime.Now.Month;
            var anoActual = ano ?? DateTime.Now.Year;

            var inicioMes = new DateTime(anoActual, mesActual, 1);
            var finMes = inicioMes.AddMonths(1).AddDays(-1);

            var gastos = await _context.Gastos
                .Where(x => x.UserId == userId && x.Fecha >= inicioMes && x.Fecha <= finMes)
                .GroupBy(x => x.Tipo)
                .Select(g => new { Tipo = g.Key, Total = g.Sum(x => x.Monto) })
                .ToListAsync();

            var gastosFijos = gastos.FirstOrDefault(x => x.Tipo == "Fijo")?.Total ?? 0;
            var gastosVariables = gastos.FirstOrDefault(x => x.Tipo == "Variable")?.Total ?? 0;
            var totalGastos = gastosFijos + gastosVariables;

            var hace3Meses = inicioMes.AddMonths(-3);
            var promedioFijos = await _context.Gastos
                .Where(x => x.UserId == userId && x.Fecha >= hace3Meses && x.Tipo == "Fijo")
                .AverageAsync(x => (decimal?)x.Monto) ?? 0;

            var promedioVariables = await _context.Gastos
               .Where(x => x.UserId == userId && x.Fecha >= hace3Meses && x.Tipo == "Variable")
               .AverageAsync(x => (decimal?)x.Monto) ?? 0;

            return new GastosTipoDto
            {
                Mes = mesActual,
                Ano = anoActual,
                GastosFijos = new GastosPorTipoDetalleDto
                {
                    Total = gastosFijos,
                    Porcentaje = totalGastos > 0 ? (gastosFijos / totalGastos) * 100 : 0,
                    Promedio = promedioFijos
                },
                GastosVariables = new GastosPorTipoDetalleDto
                {
                    Total = gastosVariables,
                    Porcentaje = totalGastos > 0 ? (gastosVariables / totalGastos) * 100 : 0,
                    Promedio = promedioVariables
                },
                TotalGastos = totalGastos
            };
        }

        public async Task<ProyeccionGastosDto> GetProyeccionAsync(string userId)
        {
            var hoy = DateTime.Now;
            var inicioMes = new DateTime(hoy.Year, hoy.Month, 1);
            var finMes = inicioMes.AddMonths(1).AddDays(-1);
            var diasTotales = finMes.Day;
            var diasTranscurridos = hoy.Day;

            var gastosActuales = await _context.Gastos
                .Where(x => x.UserId == userId && x.Fecha >= inicioMes && x.Fecha <= hoy)
                .SumAsync(x => x.Monto);

            var hace3Meses = inicioMes.AddMonths(-3);
            var promedioUltimos3Meses = await _context.Gastos
                .Where(x => x.UserId == userId && x.Fecha >= hace3Meses && x.Fecha < inicioMes)
                .GroupBy(x => new { x.Fecha.Year, x.Fecha.Month })
                .Select(g => g.Sum(x => x.Monto))
                .AverageAsync();

            var gastoEstimado = (gastosActuales / diasTranscurridos) * diasTotales;
            var diferencia = gastoEstimado - promedioUltimos3Meses;
            var porcentajeIncremento = promedioUltimos3Meses > 0
                ? (diferencia / promedioUltimos3Meses) * 100
                : 0;

            var alerta = porcentajeIncremento > 20;
            var mensaje = alerta
                ? $"Estás gastando un {Math.Abs(porcentajeIncremento):F2}% más que tu promedio"
                : porcentajeIncremento < -10
                    ? $"¡Bien! Estás gastando un {Math.Abs(porcentajeIncremento):F2}% menos que tu promedio"
                    : "Tu gasto está dentro del promedio normal";

            return new ProyeccionGastosDto
            {
                MesActual = new MesActualDto
                {
                    Mes = hoy.Month,
                    Ano = hoy.Year,
                    GastosActuales = gastosActuales,
                    DiasTranscurridos = diasTranscurridos,
                    DiasTotales = diasTotales
                },
                Proyeccion = new ProyeccionDetalleDto
                {
                    GastoEstimado = gastoEstimado,
                    PromedioUltimos3Meses = promedioUltimos3Meses,
                    Diferencia = diferencia,
                    PorcentajeIncremento = porcentajeIncremento,
                    Alerta = alerta,
                    Mensaje = mensaje
                }
            };
        }

        public async Task<CalendarioDto> GetCalendarioAsync(string userId, int mes, int ano)
        {
            var primerDia = new DateTime(ano, mes, 1);
            var ultimoDia = primerDia.AddMonths(1).AddDays(-1);

            var gastos = await _context.Gastos
                .Where(g => g.UserId == userId &&
                            g.Fecha >= primerDia &&
                            g.Fecha <= ultimoDia)
                .ToListAsync();

            var ingresos = await _context.Ingresos
                .Where(i => i.UserId == userId &&
                            i.Fecha >= primerDia &&
                            i.Fecha <= ultimoDia)
                .ToListAsync();

            var categorias = await _context.Categorias
                .Where(c => c.UserId == userId)
                .ToDictionaryAsync(c => c.Id, c => c.Nombre);

            var dias = new List<DiaCalendarioDto>();

            for (var fecha = primerDia; fecha <= ultimoDia; fecha = fecha.AddDays(1))
            {
                var gastosDelDia = gastos.Where(g => g.Fecha.Date == fecha.Date).ToList();
                var ingresosDelDia = ingresos.Where(i => i.Fecha.Date == fecha.Date).ToList();

                var totalGastos = gastosDelDia.Sum(g => g.Monto);
                var totalIngresos = ingresosDelDia.Sum(i => i.Monto);

                var transacciones = new List<TransaccionSummaryDto>();

                transacciones.AddRange(gastosDelDia.Select(g => new TransaccionSummaryDto
                {
                    Id = g.Id,
                    Tipo = "Gasto",
                    Descripcion = g.Descripcion,
                    Monto = g.Monto,
                    CategoriaNombre = categorias.GetValueOrDefault(g.CategoriaId)
                }));

                transacciones.AddRange(ingresosDelDia.Select(i => new TransaccionSummaryDto
                {
                    Id = i.Id,
                    Tipo = "Ingreso",
                    Descripcion = i.Descripcion,
                    Monto = i.Monto,
                    CategoriaNombre = categorias.GetValueOrDefault(i.CategoriaId)
                }));

                if (transacciones.Any())
                {
                    dias.Add(new DiaCalendarioDto
                    {
                        Fecha = fecha,
                        TotalIngresos = totalIngresos,
                        TotalGastos = totalGastos,
                        Balance = totalIngresos - totalGastos,
                        CantidadTransacciones = transacciones.Count,
                        Transacciones = transacciones
                    });
                }
            }

            return new CalendarioDto
            {
                Mes = mes,
                Ano = ano,
                Dias = dias
            };
        }

        public async Task<ComparacionPeriodosDto> CompararPeriodosAsync(string userId, DateTime fecha1Inicio, DateTime fecha1Fin, DateTime fecha2Inicio, DateTime fecha2Fin)
        {
            // Periodo 1
            var ingresos1 = await _context.Ingresos
                .Where(i => i.UserId == userId && i.Fecha >= fecha1Inicio && i.Fecha <= fecha1Fin)
                .ToListAsync();
            var gastos1 = await _context.Gastos
                .Where(g => g.UserId == userId && g.Fecha >= fecha1Inicio && g.Fecha <= fecha1Fin)
                .ToListAsync();

            var totalIngresos1 = ingresos1.Sum(i => i.Monto);
            var totalGastos1 = gastos1.Sum(g => g.Monto);

            // Periodo 2
            var ingresos2 = await _context.Ingresos
                .Where(i => i.UserId == userId && i.Fecha >= fecha2Inicio && i.Fecha <= fecha2Fin)
                .ToListAsync();
            var gastos2 = await _context.Gastos
                .Where(g => g.UserId == userId && g.Fecha >= fecha2Inicio && g.Fecha <= fecha2Fin)
                .ToListAsync();

            var totalIngresos2 = ingresos2.Sum(i => i.Monto);
            var totalGastos2 = gastos2.Sum(g => g.Monto);

            var diffIngresos = totalIngresos2 - totalIngresos1;
            var diffGastos = totalGastos2 - totalGastos1;
            var diffBalance = (totalIngresos2 - totalGastos2) - (totalIngresos1 - totalGastos1);

            var pctIngresos = totalIngresos1 > 0 ? (diffIngresos / totalIngresos1) * 100 : 0;
            var pctGastos = totalGastos1 > 0 ? (diffGastos / totalGastos1) * 100 : 0;

            return new ComparacionPeriodosDto
            {
                Periodo1 = new PeriodoFinancieroDto
                {
                    FechaInicio = fecha1Inicio,
                    FechaFin = fecha1Fin,
                    TotalIngresos = totalIngresos1,
                    TotalGastos = totalGastos1,
                    Balance = totalIngresos1 - totalGastos1,
                    CantidadIngresos = ingresos1.Count,
                    CantidadGastos = gastos1.Count
                },
                Periodo2 = new PeriodoFinancieroDto
                {
                    FechaInicio = fecha2Inicio,
                    FechaFin = fecha2Fin,
                    TotalIngresos = totalIngresos2,
                    TotalGastos = totalGastos2,
                    Balance = totalIngresos2 - totalGastos2,
                    CantidadIngresos = ingresos2.Count,
                    CantidadGastos = gastos2.Count
                },
                DiferenciaIngresos = diffIngresos,
                DiferenciaGastos = diffGastos,
                DiferenciaBalance = diffBalance,
                PorcentajeCambioIngresos = pctIngresos,
                PorcentajeCambioGastos = pctGastos
            };
        }
    }
}
