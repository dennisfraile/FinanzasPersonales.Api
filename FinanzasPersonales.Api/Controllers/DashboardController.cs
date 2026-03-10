using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using FinanzasPersonales.Api.Dtos;
using System.Security.Claims;
using FinanzasPersonales.Api.Services;

namespace FinanzasPersonales.Api.Controllers
{
    /// <summary>
    /// API para dashboard y visualización de datos financieros.
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class DashboardController : ControllerBase
    {
        private readonly IDashboardService _dashboardService;

        public DashboardController(IDashboardService dashboardService)
        {
            _dashboardService = dashboardService;
        }

        /// <summary>
        /// Obtiene datos completos del dashboard
        /// </summary>
        [HttpGet]
        [ProducesResponseType(typeof(DashboardDto), StatusCodes.Status200OK)]
        public async Task<ActionResult<DashboardDto>> GetDashboard([FromQuery] int? mes = null, [FromQuery] int? ano = null)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            var resultado = await _dashboardService.GetDashboardAsync(userId, mes, ano);

            return Ok(resultado);
        }

        /// <summary>
        /// Obtiene gráfica de ingresos vs gastos por mes
        /// </summary>
        [HttpGet("grafica/ingresos-vs-gastos")]
        [ProducesResponseType(typeof(GraficaDto), StatusCodes.Status200OK)]
        public async Task<ActionResult<GraficaDto>> GetGraficaIngresosVsGastos([FromQuery] int meses = 6)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            var resultado = await _dashboardService.GetGraficaIngresosVsGastosAsync(userId, meses);

            return Ok(resultado);
        }

        /// <summary>
        /// Obtiene gráfica de gastos por categoría (gráfica de pie)
        /// </summary>
        [HttpGet("grafica/gastos-por-categoria")]
        [ProducesResponseType(typeof(GraficaDto), StatusCodes.Status200OK)]
        public async Task<ActionResult<GraficaDto>> GetGraficaGastosPorCategoria([FromQuery] int? mes = null, [FromQuery] int? ano = null)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            var resultado = await _dashboardService.GetGraficaGastosPorCategoriaAsync(userId, mes, ano);

            return Ok(resultado);
        }

        /// <summary>
        /// Obtiene gráfica de progreso de metas
        /// </summary>
        [HttpGet("grafica/progreso-metas")]
        [ProducesResponseType(typeof(GraficaDto), StatusCodes.Status200OK)]
        public async Task<ActionResult<GraficaDto>> GetGraficaProgresoMetas()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            var resultado = await _dashboardService.GetGraficaProgresoMetasAsync(userId);

            return Ok(resultado);
        }

        /// <summary>
        /// Obtiene métricas mejoradas para dashboard con gráficas
        /// </summary>
        [HttpGet("metrics")]
        public async Task<ActionResult<DashboardMetricsDto>> GetMetrics()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId)) return Unauthorized();

            var resultado = await _dashboardService.GetMetricsAsync(userId);

            return Ok(resultado);
        }
    }
}
