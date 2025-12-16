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
    }
}
