using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using FinanzasPersonales.Api.Dtos;
using System.Security.Claims;
using FinanzasPersonales.Api.Services;

namespace FinanzasPersonales.Api.Controllers
{
    /// <summary>
    /// API para gestión de reportes y analíticas financieras.
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class ReportesController : ControllerBase
    {
        private readonly IReportesService _reportesService;

        public ReportesController(IReportesService reportesService)
        {
            _reportesService = reportesService;
        }

        /// <summary>
        /// Obtiene un reporte de gastos agrupados por categoría para un mes específico.
        /// </summary>
        [HttpGet("gastos-por-categoria")]
        [ProducesResponseType(typeof(List<GastoPorCategoriaDto>), StatusCodes.Status200OK)]
        public async Task<ActionResult<List<GastoPorCategoriaDto>>> GetGastosPorCategoria(
            [FromQuery] int? mes = null,
            [FromQuery] int? ano = null)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var resultado = await _reportesService.GetGastosPorCategoriaAsync(userId!, mes, ano);

            return Ok(resultado);
        }

        /// <summary>
        /// Obtiene la evolución mensual de ingresos y gastos.
        /// </summary>
        [HttpGet("evolucion-mensual")]
        [ProducesResponseType(typeof(List<EvolucionMensualDto>), StatusCodes.Status200OK)]
        public async Task<ActionResult<List<EvolucionMensualDto>>> GetEvolucionMensual([FromQuery] int meses = 6)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var resultado = await _reportesService.GetEvolucionMensualAsync(userId!, meses);

            return Ok(resultado);
        }

        /// <summary>
        /// Compara dos períodos financieros específicos.
        /// </summary>
        [HttpGet("comparativa-periodos")]
        [ProducesResponseType(typeof(ComparativaPeriodosDto), StatusCodes.Status200OK)]
        public async Task<ActionResult<ComparativaPeriodosDto>> GetComparativaPeriodos(
            [FromQuery] int? mesActual = null,
            [FromQuery] int? anoActual = null,
            [FromQuery] int? mesAnterior = null,
            [FromQuery] int? anoAnterior = null)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var resultado = await _reportesService.GetComparativaPeriodosAsync(userId!, mesActual, anoActual, mesAnterior, anoAnterior);

            return Ok(resultado);
        }

        /// <summary>
        /// Obtiene estadísticas generales para un rango de fechas.
        /// </summary>
        [HttpGet("resumen-general")]
        [ProducesResponseType(typeof(ResumenGeneralDto), StatusCodes.Status200OK)]
        public async Task<ActionResult<ResumenGeneralDto>> GetResumenGeneral(
            [FromQuery] DateTime? desde = null,
            [FromQuery] DateTime? hasta = null)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var resultado = await _reportesService.GetResumenGeneralAsync(userId!, desde, hasta);

            return Ok(resultado);
        }

        /// <summary>
        /// Obtiene tendencias mensuales de ingresos y gastos
        /// </summary>
        [HttpGet("tendencias")]
        [ProducesResponseType(typeof(TendenciasMensualesDto), StatusCodes.Status200OK)]
        public async Task<ActionResult<TendenciasMensualesDto>> GetTendencias([FromQuery] int meses = 6)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var resultado = await _reportesService.GetTendenciasAsync(userId!, meses);

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

            var resultado = await _reportesService.GetComparativaAsync(userId!, mes, ano);

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

            var resultado = await _reportesService.GetTopCategoriasAsync(userId!, mes, ano, limite);

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

            var resultado = await _reportesService.GetGastosTipoAsync(userId!, mes, ano);

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

            var resultado = await _reportesService.GetProyeccionAsync(userId!);

            return Ok(resultado);
        }

        /// <summary>
        /// Obtiene datos de calendario con transacciones agrupadas por día para un mes específico.
        /// </summary>
        [HttpGet("calendario")]
        public async Task<ActionResult<CalendarioDto>> GetCalendario(int mes, int ano)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId)) return Unauthorized();

            var resultado = await _reportesService.GetCalendarioAsync(userId, mes, ano);

            return Ok(resultado);
        }

        [HttpGet("comparar-periodos")]
        public async Task<ActionResult<ComparacionPeriodosDto>> CompararPeriodos(
            [FromQuery] DateTime fecha1Inicio,
            [FromQuery] DateTime fecha1Fin,
            [FromQuery] DateTime fecha2Inicio,
            [FromQuery] DateTime fecha2Fin)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId)) return Unauthorized();

            var resultado = await _reportesService.CompararPeriodosAsync(userId, fecha1Inicio, fecha1Fin, fecha2Inicio, fecha2Fin);

            return Ok(resultado);
        }
    }
}
