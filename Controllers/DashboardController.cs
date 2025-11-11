using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using FinanzasPersonales.Api.Data;
using FinanzasPersonales.Api.Dtos;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;

namespace FinanzasPersonales.Api.Controllers
{
    /// <summary>
    /// Proporciona endpoints para cálculos de resumen financiero y lógica de negocio.
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    [Authorize] // ¡Todo este controlador está protegido!
    public class DashboardController : ControllerBase
    {
        private readonly FinanzasDbContext _context;

        public DashboardController(FinanzasDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Obtiene el resumen financiero quincenal y mensual para el usuario autenticado.
        /// </summary>
        /// <remarks>
        /// Reemplaza todas las fórmulas del Dashboard de Excel.
        /// Calcula el saldo disponible basado en la lógica de "vivir con ingresos anteriores"
        /// y el flujo libre mensual total para el cálculo de metas.
        /// </remarks>
        [HttpGet("resumen-quincenal")]
        [ProducesResponseType(typeof(DashboardResumenDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult<DashboardResumenDto>> GetResumenQuincenal()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null)
            {
                return Unauthorized();
            }

            // --- 1. Definir Rangos de Fechas (Reemplaza las celdas H1-H6) ---
            var hoy = DateTime.Today;
            var inicioMesActual = new DateTime(hoy.Year, hoy.Month, 1);
            var finMesActual = inicioMesActual.AddMonths(1).AddDays(-1);
            var finQ1Actual = new DateTime(hoy.Year, hoy.Month, 15);
            var inicioQ2Actual = finQ1Actual.AddDays(1);

            // Período anterior (para ingresos)
            var inicioMesAnterior = inicioMesActual.AddMonths(-1);
            var finQ1Anterior = new DateTime(inicioMesAnterior.Year, inicioMesAnterior.Month, 15);
            var inicioQ2Anterior = finQ1Anterior.AddDays(1);
            var finMesAnterior = inicioMesActual.AddDays(-1);

            // --- 2. Lógica de "Período Actual" (¿Estoy en Q1 o Q2?) ---
            string periodoActualNombre;
            DateTime inicioPeriodoGastos;
            DateTime finPeriodoGastos;
            DateTime inicioPeriodoIngresos;
            DateTime finPeriodoIngresos;

            if (hoy <= finQ1Actual)
            {
                // --- Estamos en Q1 (Gastos 1-15 NOV) ---
                periodoActualNombre = $"Quincena 1 ({inicioMesActual:dd/MM} - {finQ1Actual:dd/MM})";
                inicioPeriodoGastos = inicioMesActual;
                finPeriodoGastos = finQ1Actual;

                // Usamos ingresos de Q2 del mes anterior (Ingresos 16-31 OCT)
                inicioPeriodoIngresos = inicioQ2Anterior;
                finPeriodoIngresos = finMesAnterior;
            }
            else
            {
                // --- Estamos en Q2 (Gastos 16-30 NOV) ---
                periodoActualNombre = $"Quincena 2 ({inicioQ2Actual:dd/MM} - {finMesActual:dd/MM})";
                inicioPeriodoGastos = inicioQ2Actual;
                finPeriodoGastos = finMesActual;

                // Usamos ingresos de Q1 de este mes (Ingresos 1-15 NOV)
                inicioPeriodoIngresos = inicioMesActual;
                finPeriodoIngresos = finQ1Actual;
            }

            // --- 3. Consultar la BD (Reemplaza SUMAR.SI.CONJUNTO) ---

            // 3.1: Ingresos ASIGNADOS para este período
            var ingresosAsignados = await _context.Ingresos
                .Where(i => i.UserId == userId &&
                            i.Fecha >= inicioPeriodoIngresos &&
                            i.Fecha <= finPeriodoIngresos)
                .SumAsync(i => i.Monto);

            // 3.2: Gastos PAGADOS en este período
            var gastosFijosPagados = await _context.Gastos
                .Where(g => g.UserId == userId &&
                            g.Tipo == "Fijo" &&
                            g.Fecha >= inicioPeriodoGastos &&
                            g.Fecha <= finPeriodoGastos)
                .SumAsync(g => g.Monto);

            var gastosVariablesPagados = await _context.Gastos
                .Where(g => g.UserId == userId &&
                            g.Tipo == "Variable" &&
                            g.Fecha >= inicioPeriodoGastos &&
                            g.Fecha <= finPeriodoGastos)
                .SumAsync(g => g.Monto);

            // --- 4. Calcular el Resumen del Período (El 10% y Saldo) ---
            var ahorroBase = ingresosAsignados * 0.10m; // ¡Tu lógica del 10%! 'm' es para decimal
            var saldoDisponible = ingresosAsignados - ahorroBase - gastosFijosPagados - gastosVariablesPagados;

            // --- 5. Calcular Flujo Libre Mensual (Para Metas - Reemplaza E10) ---
            // Esto es DIFERENTE del saldo. Esto calcula el sobrante REAL del mes actual.

            // 5.1: Ingresos TOTALES del mes actual
            var ingresosTotalesMes = await _context.Ingresos
                .Where(i => i.UserId == userId &&
                            i.Fecha >= inicioMesActual &&
                            i.Fecha <= finMesActual)
                .SumAsync(i => i.Monto);

            // 5.2: Gastos TOTALES del mes actual
            var gastosTotalesMes = await _context.Gastos
                .Where(g => g.UserId == userId &&
                            g.Fecha >= inicioMesActual &&
                            g.Fecha <= finMesActual)
                .SumAsync(g => g.Monto);

            // 5.3: Ahorro TOTAL del mes actual
            var ahorroTotalMes = ingresosTotalesMes * 0.10m;

            var flujoLibreMensual = ingresosTotalesMes - gastosTotalesMes - ahorroTotalMes;

            // --- 6. Construir y Devolver el Objeto de Respuesta (DTO) ---
            var resumenDto = new DashboardResumenDto
            {
                PeriodoActual = periodoActualNombre,
                IngresosAsignados = ingresosAsignados,
                AhorroBaseCalculado = ahorroBase,
                GastosFijosPagados = gastosFijosPagados,
                GastosVariablesPagados = gastosVariablesPagados,
                SaldoDisponiblePeriodo = saldoDisponible,
                FlujoLibreMensual = flujoLibreMensual
            };

            return Ok(resumenDto);
        }
    }
}
