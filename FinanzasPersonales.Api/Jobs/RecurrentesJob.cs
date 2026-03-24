using FinanzasPersonales.Api.Data;
using FinanzasPersonales.Api.Services;
using Microsoft.EntityFrameworkCore;

namespace FinanzasPersonales.Api.Jobs
{
    /// <summary>
    /// Job de Hangfire que genera automáticamente transacciones desde ingresos y gastos recurrentes
    /// cuando su ProximaFecha ya pasó.
    /// </summary>
    public class RecurrentesJob
    {
        private readonly FinanzasDbContext _context;
        private readonly IIngresosRecurrentesService _ingresosService;
        private readonly IGastosRecurrentesService _gastosService;
        private readonly IMetasService _metasService;
        private readonly ILogger<RecurrentesJob> _logger;

        public RecurrentesJob(
            FinanzasDbContext context,
            IIngresosRecurrentesService ingresosService,
            IGastosRecurrentesService gastosService,
            IMetasService metasService,
            ILogger<RecurrentesJob> logger)
        {
            _context = context;
            _ingresosService = ingresosService;
            _gastosService = gastosService;
            _metasService = metasService;
            _logger = logger;
        }

        /// <summary>
        /// Genera todas las transacciones pendientes de ingresos y gastos recurrentes
        /// para todos los usuarios.
        /// </summary>
        public async Task GenerarTransaccionesRecurrentesAsync()
        {
            _logger.LogInformation("=== Iniciando job de generación de transacciones recurrentes ===");

            var totalIngresosGenerados = 0;
            var totalGastosGenerados = 0;

            // Obtener usuarios con ingresos recurrentes pendientes
            var usersConIngresosPendientes = await _context.IngresosRecurrentes
                .Where(ir => ir.Activo && ir.ProximaFecha <= DateTime.UtcNow)
                .Select(ir => ir.UserId)
                .Distinct()
                .ToListAsync();

            foreach (var userId in usersConIngresosPendientes)
            {
                try
                {
                    var generados = await _ingresosService.GenerarPendientesAsync(userId);
                    totalIngresosGenerados += generados;
                    if (generados > 0)
                        _logger.LogInformation("Usuario {UserId}: {Count} ingreso(s) recurrente(s) generado(s)", userId, generados);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error generando ingresos recurrentes para usuario {UserId}", userId);
                }
            }

            // Obtener usuarios con gastos recurrentes pendientes
            var usersConGastosPendientes = await _context.GastosRecurrentes
                .Where(gr => gr.Activo && gr.ProximaFecha <= DateTime.UtcNow)
                .Select(gr => gr.UserId)
                .Distinct()
                .ToListAsync();

            foreach (var userId in usersConGastosPendientes)
            {
                try
                {
                    var generados = await _gastosService.GenerarPendientesAsync(userId);
                    totalGastosGenerados += generados;
                    if (generados > 0)
                        _logger.LogInformation("Usuario {UserId}: {Count} gasto(s) recurrente(s) generado(s)", userId, generados);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error generando gastos recurrentes para usuario {UserId}", userId);
                }
            }

            // Abonos automáticos a metas
            var totalAbonosGenerados = 0;
            var usersConAbonosPendientes = await _context.Metas
                .Where(m => m.AbonoAutomatico
                    && m.ProximoAbono.HasValue && m.ProximoAbono <= DateTime.UtcNow
                    && m.AhorroActual < m.MontoTotal)
                .Select(m => m.UserId)
                .Distinct()
                .ToListAsync();

            foreach (var userId in usersConAbonosPendientes)
            {
                try
                {
                    var generados = await _metasService.GenerarAbonosAutomaticosAsync(userId);
                    totalAbonosGenerados += generados;
                    if (generados > 0)
                        _logger.LogInformation("Usuario {UserId}: {Count} abono(s) automático(s) a metas generado(s)", userId, generados);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error generando abonos automáticos para usuario {UserId}", userId);
                }
            }

            _logger.LogInformation(
                "=== Job de recurrentes completado: {Ingresos} ingreso(s), {Gastos} gasto(s), {Abonos} abono(s) a metas generados ===",
                totalIngresosGenerados, totalGastosGenerados, totalAbonosGenerados);
        }
    }
}
