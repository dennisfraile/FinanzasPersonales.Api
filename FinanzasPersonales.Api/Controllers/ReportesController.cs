using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using FinanzasPersonales.Api.Data;
using FinanzasPersonales.Api.Dtos;
using FinanzasPersonales.Api.Models;
using System.Security.Claims;
using System.Globalization;

namespace FinanzasPersonales.Api.Controllers
{
    /// <summary>
    /// API para gestión de reportes y anál íticas financieras.
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class ReportesController : ControllerBase
    {
        private readonly FinanzasDbContext _context;

        public ReportesController(FinanzasDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Obtiene un reporte de gastos agrupados por categoría para un mes específico.
        /// </summary>
        /// <param name="mes">Mes a analizar (1-12). Default: mes actual</param>
        /// <param name="ano">Año a analizar. Default: año actual</param>
        [HttpGet("gastos-por-categoria")]
        [ProducesResponseType(typeof(List<GastoPorCategoriaDto>), StatusCodes.Status200OK)]
        public async Task<ActionResult<List<GastoPorCategoriaDto>>> GetGastosPorCategoria(
            [FromQuery] int? mes = null,
            [FromQuery] int? ano = null)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var mesActual = mes ?? DateTime.Now.Month;
            var anoActual = ano ?? DateTime.Now.Year;

            var inicioMes = new DateTime(anoActual, mesActual, 1);
            var finMes = inicioMes.AddMonths(1).AddDays(-1);

            // Obtener gastos del mes agrupados por categoría
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

            var resultado = gastosPorCategoria.Select(x => new GastoPorCategoriaDto
            {
                CategoriaId = x.CategoriaId,
                CategoriaNombre = x.CategoriaNombre,
                TotalGastado = x.TotalGastado,
                CantidadTransacciones = x.CantidadTransacciones,
                PorcentajeDelTotal = totalGeneral > 0 ? (x.TotalGastado / totalGeneral) * 100 : 0
            }).ToList();

            return Ok(resultado);
        }

        /// <summary>
        /// Obtiene la evolución mensual de ingresos y gastos.
        /// </summary>
        /// <param name="meses">Cantidad de meses hacia atrás a analizar. Default: 6, Máximo: 12</param>
        [HttpGet("evolucion-mensual")]
        [ProducesResponseType(typeof(List<EvolucionMensualDto>), StatusCodes.Status200OK)]
        public async Task<ActionResult<List<EvolucionMensualDto>>> GetEvolucionMensual([FromQuery] int meses = 6)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            // Limitar a máximo 12 meses
            meses = Math.Min(meses, 12);

            var resultado = new List<EvolucionMensualDto>();
            var fechaActual = DateTime.Now;

            for (int i = meses - 1; i >= 0; i--)
            {
                var mes = fechaActual.AddMonths(-i);
                var inicioMes = new DateTime(mes.Year, mes.Month, 1);
                var finMes = inicioMes.AddMonths(1).AddDays(-1);

                var ingresos = await _context.Ingresos
                    .Where(x => x.UserId == userId &&
                               x.Fecha >= inicioMes &&
                               x.Fecha <= finMes)
                    .SumAsync(x => x.Monto);

                var gastos = await _context.Gastos
                    .Where(x => x.UserId == userId &&
                               x.Fecha >= inicioMes &&
                               x.Fecha <= finMes)
                    .SumAsync(x => x.Monto);

                var ahorro = ingresos * 0.10m;
                var balance = ingresos - gastos;

                resultado.Add(new EvolucionMensualDto
                {
                    Mes = mes.Month,
                    Ano = mes.Year,
                    Periodo = mes.ToString("MMMM yyyy", new CultureInfo("es-ES")),
                    TotalIngresos = ingresos,
                    TotalGastos = gastos,
                    AhorroCalculado = ahorro,
                    Balance = balance
                });
            }

            return Ok(resultado);
        }

        /// <summary>
        /// Compara dos períodos financieros específicos.
        /// </summary>
        /// <param name="mesActual">Mes del período actual (1-12)</param>
        /// <param name="anoActual">Año del período actual</param>
        /// <param name="mesAnterior">Mes del período anterior (1-12)</param>
        /// <param name="anoAnterior">Año del período anterior</param>
        [HttpGet("comparativa-periodos")]
        [ProducesResponseType(typeof(ComparativaPeriodosDto), StatusCodes.Status200OK)]
        public async Task<ActionResult<ComparativaPeriodosDto>> GetComparativaPeriodos(
            [FromQuery] int? mesActual = null,
            [FromQuery] int? anoActual = null,
            [FromQuery] int? mesAnterior = null,
            [FromQuery] int? anoAnterior = null)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            // Defaults: comparar mes actual vs mes anterior
            var fechaActual = DateTime.Now;
            var mesA = mesActual ?? fechaActual.Month;
            var anoA = anoActual ?? fechaActual.Year;

            var fechaAnt = fechaActual.AddMonths(-1);
            var mesAnt = mesAnterior ?? fechaAnt.Month;
            var anoAnt = anoAnterior ?? fechaAnt.Year;

            // Período actual
            var inicioMA = new DateTime(anoA, mesA, 1);
            var finMA = inicioMA.AddMonths(1).AddDays(-1);

            var ingresosActual = await _context.Ingresos
                .Where(x => x.UserId == userId && x.Fecha >= inicioMA && x.Fecha <= finMA)
                .SumAsync(x => x.Monto);

            var gastosActual = await _context.Gastos
                .Where(x => x.UserId == userId && x.Fecha >= inicioMA && x.Fecha <= finMA)
                .SumAsync(x => x.Monto);

            // Período anterior
            var inicioMAnt = new DateTime(anoAnt, mesAnt, 1);
            var finMAnt = inicioMAnt.AddMonths(1).AddDays(-1);

            var ingresosAnterior = await _context.Ingresos
                .Where(x => x.UserId == userId && x.Fecha >= inicioMAnt && x.Fecha <= finMAnt)
                .SumAsync(x => x.Monto);

            var gastosAnterior = await _context.Gastos
                .Where(x => x.UserId == userId && x.Fecha >= inicioMAnt && x.Fecha <= finMAnt)
                .SumAsync(x => x.Monto);

            var resultado = new ComparativaPeriodosDto
            {
                PeriodoActual = new PeriodoFinanciero
                {
                    Descripcion = new DateTime(anoA, mesA, 1).ToString("MMMM yyyy", new CultureInfo("es-ES")),
                    TotalIngresos = ingresosActual,
                    TotalGastos = gastosActual,
                    Balance = ingresosActual - gastosActual
                },
                PeriodoAnterior = new PeriodoFinanciero
                {
                    Descripcion = new DateTime(anoAnt, mesAnt, 1).ToString("MMMM yyyy", new CultureInfo("es-ES")),
                    TotalIngresos = ingresosAnterior,
                    TotalGastos = gastosAnterior,
                    Balance = ingresosAnterior - gastosAnterior
                },
                DiferenciaIngresos = ingresosActual - ingresosAnterior,
                DiferenciaGastos = gastosActual - gastosAnterior,
                DiferenciaBalance = (ingresosActual - gastosActual) - (ingresosAnterior - gastosAnterior),
                PorcentajeCambioIngresos = ingresosAnterior != 0
                    ? ((ingresosActual - ingresosAnterior) / ingresosAnterior) * 100
                    : 0,
                PorcentajeCambioGastos = gastosAnterior != 0
                    ? ((gastosActual - gastosAnterior) / gastosAnterior) * 100
                    : 0
            };

            return Ok(resultado);
        }

        /// <summary>
        /// Obtiene estadísticas generales para un rango de fechas.
        /// </summary>
        /// <param name="desde">Fecha inicial del rango. Default: inicio del año actual</param>
        /// <param name="hasta">Fecha final del rango. Default: fecha actual</param>
        [HttpGet("resumen-general")]
        [ProducesResponseType(typeof(ResumenGeneralDto), StatusCodes.Status200OK)]
        public async Task<ActionResult<ResumenGeneralDto>> GetResumenGeneral(
            [FromQuery] DateTime? desde = null,
            [FromQuery] DateTime? hasta = null)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

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

            var resultado = new ResumenGeneralDto
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

            return Ok(resultado);
        }

        /// <summary>
        /// Obtiene tendencias mensuales de ingresos y gastos
        /// </summary>
        /// <param name="meses">Cantidad de meses a analizar (default: 6, máx: 12)</param>
        [HttpGet("tendencias")]
        [ProducesResponseType(typeof(TendenciasMensualesDto), StatusCodes.Status200OK)]
        public async Task<ActionResult<TendenciasMensualesDto>> GetTendencias([FromQuery] int meses = 6)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            meses = Math.Min(meses, 12);

            var fechaActual = DateTime.Now;
            var fechaInicio = fechaActual.AddMonths(-(meses - 1));
            var inicioMes = new DateTime(fechaInicio.Year, fechaInicio.Month, 1);

            var datos = new List<DatoMensualDto>();

            for (int i = 0; i < meses; i++)
            {
                var mes = inicioMes.AddMonths(i);
                var finMes = mes.AddMonths(1).AddDays(-1);

                var ingresos = await _context.Ingresos
                    .Where(x => x.UserId == userId && x.Fecha >= mes && x.Fecha <= finMes)
                    .SumAsync(x => x.Monto);

                var gastos = await _context.Gastos
                    .Where(x => x.UserId == userId && x.Fecha >= mes && x.Fecha <= finMes)
                    .SumAsync(x => x.Monto);

                datos.Add(new DatoMensualDto
                {
                    Mes = mes.Month,
                    Ano = mes.Year,
                    Periodo = mes.ToString("MMM yyyy", new CultureInfo("es-ES")),
                    TotalIngresos = ingresos,
                    TotalGastos = gastos,
                    Balance = ingresos - gastos
                });
            }

            var resultado = new TendenciasMensualesDto
            {
                Periodo = new PeriodoDto
                {
                    Inicio = inicioMes,
                    Fin = fechaActual
                },
                Datos = datos
            };

            return Ok(resultado);
        }

        /// <summary>
        /// Compara el mes actual con el mes anterior
        /// </summary>
        [HttpGet("comparativa")]
        [ProducesResponseType(typeof(ComparativaMesDto), StatusCodes.Status200OK)]
        public async Task<ActionResult<ComparativaMesDto>> GetComparativa(
            [FromQuery] int? mes = null,
            [FromQuery] int? ano = null)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

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
            var ingresosActual = await _context.Ingresos
                .Where(x => x.UserId == userId && x.Fecha >= inicioActual && x.Fecha <= finActual)
                .SumAsync(x => x.Monto);

            var gastosActual = await _context.Gastos
                .Where(x => x.UserId == userId && x.Fecha >= inicioActual && x.Fecha <= finActual)
                .SumAsync(x => x.Monto);

            // Datos mes anterior
            var ingresosAnterior = await _context.Ingresos
                .Where(x => x.UserId == userId && x.Fecha >= inicioAnterior && x.Fecha <= finAnterior)
                .SumAsync(x => x.Monto);

            var gastosAnterior = await _context.Gastos
                .Where(x => x.UserId == userId && x.Fecha >= inicioAnterior && x.Fecha <= finAnterior)
                .SumAsync(x => x.Monto);

            // Comparativa por categorías
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

            var balanceActual = ingresosActual - gastosActual;
            var balanceAnterior = ingresosAnterior - gastosAnterior;

            var resultado = new ComparativaMesDto
            {
                MesActual = new ResumenMesDto
                {
                    Mes = mesActual,
                    Ano = anoActual,
                    TotalIngresos = ingresosActual,
                    TotalGastos = gastosActual,
                    Balance = balanceActual
                },
                MesAnterior = new ResumenMesDto
                {
                    Mes = mesAnt.Month,
                    Ano = mesAnt.Year,
                    TotalIngresos = ingresosAnterior,
                    TotalGastos = gastosAnterior,
                    Balance = balanceAnterior
                },
                Cambios = new CambiosDto
                {
                    IngresosPorcentaje = ingresosAnterior > 0 ? ((ingresosActual - ingresosAnterior) / ingresosAnterior) * 100 : 0,
                    GastosPorcentaje = gastosAnterior > 0 ? ((gastosActual - gastosAnterior) / gastosAnterior) * 100 : 0,
                    BalancePorcentaje = balanceAnterior > 0 ? ((balanceActual - balanceAnterior) / balanceAnterior) * 100 : 0
                },
                Categorias = comparativaCategorias
            };

            return Ok(resultado);
        }

        /// <summary>
        /// Obtiene las categorías con más gastos
        /// </summary>
        [HttpGet("top-categorias")]
        [ProducesResponseType(typeof(TopCategoriasDto), StatusCodes.Status200OK)]
        public async Task<ActionResult<TopCategoriasDto>> GetTopCategorias(
            [FromQuery] int? mes = null,
            [FromQuery] int? ano = null,
            [FromQuery] int limite = 5)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

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

            var resultado = new TopCategoriasDto
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

            return Ok(resultado);
        }

        /// <summary>
        /// Analiza gastos fijos vs variables
        /// </summary>
        [HttpGet("gastos-tipo")]
        [ProducesResponseType(typeof(GastosTipoDto), StatusCodes.Status200OK)]
        public async Task<ActionResult<GastosTipoDto>> GetGastosTipo(
            [FromQuery] int? mes = null,
            [FromQuery] int? ano = null)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

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

            // Calcular promedio de últimos 3 meses
            var hace3Meses = inicioMes.AddMonths(-3);
            var promedioFijos = await _context.Gastos
                .Where(x => x.UserId == userId && x.Fecha >= hace3Meses && x.Tipo == "Fijo")
                .AverageAsync(x => (decimal?)x.Monto) ?? 0;

            var promedioVariables = await _context.Gastos
               .Where(x => x.UserId == userId && x.Fecha >= hace3Meses && x.Tipo == "Variable")
               .AverageAsync(x => (decimal?)x.Monto) ?? 0;

            var resultado = new GastosTipoDto
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

            return Ok(resultado);
        }

        /// <summary>
        /// Proyecta los gastos del mes actual
        /// </summary>
        [HttpGet("proyeccion")]
        [ProducesResponseType(typeof(ProyeccionGastosDto), StatusCodes.Status200OK)]
        public async Task<ActionResult<ProyeccionGastosDto>> GetProyeccion()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var hoy = DateTime.Now;
            var inicioMes = new DateTime(hoy.Year, hoy.Month, 1);
            var finMes = inicioMes.AddMonths(1).AddDays(-1);
            var diasTotales = finMes.Day;
            var diasTranscurridos = hoy.Day;

            // Gastos del mes actual
            var gastosActuales = await _context.Gastos
                .Where(x => x.UserId == userId && x.Fecha >= inicioMes && x.Fecha <= hoy)
                .SumAsync(x => x.Monto);

            // Promedio últimos 3 meses
            var hace3Meses = inicioMes.AddMonths(-3);
            var promedioUltimos3Meses = await _context.Gastos
                .Where(x => x.UserId == userId && x.Fecha >= hace3Meses && x.Fecha < inicioMes)
                .GroupBy(x => new { x.Fecha.Year, x.Fecha.Month })
                .Select(g => g.Sum(x => x.Monto))
                .AverageAsync();

            // Proyección
            var gastoEstimado = (gastosActuales / diasTranscurridos) * diasTotales;
            var diferencia = gastoEstimado - promedioUltimos3Meses;
            var porcentajeIncremento = promedioUltimos3Meses > 0
                ? (diferencia / promedioUltimos3Meses) * 100
                : 0;

            var alerta = porcentajeIncremento > 20; // Alerta si es 20% más alto
            var mensaje = alerta
                ? $"Estás gastando un {Math.Abs(porcentajeIncremento):F2}% más que tu promedio"
                : porcentajeIncremento < -10
                    ? $"¡Bien! Estás gastando un {Math.Abs(porcentajeIncremento):F2}% menos que tu promedio"
                    : "Tu gasto está dentro del promedio normal";

            var resultado = new ProyeccionGastosDto
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

            return Ok(resultado);
        }
    }
}
