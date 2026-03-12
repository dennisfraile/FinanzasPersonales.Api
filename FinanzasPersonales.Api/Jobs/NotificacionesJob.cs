using FinanzasPersonales.Api.Data;
using FinanzasPersonales.Api.Services;
using Microsoft.EntityFrameworkCore;

namespace FinanzasPersonales.Api.Jobs
{
    /// <summary>
    /// Job de Hangfire que verifica alertas de presupuestos y metas diariamente.
    /// </summary>
    public class NotificacionesJob
    {
        private readonly FinanzasDbContext _context;
        private readonly IEmailService _emailService;
        private readonly INotificacionService _notificacionService;
        private readonly ILogger<NotificacionesJob> _logger;

        public NotificacionesJob(
            FinanzasDbContext context,
            IEmailService emailService,
            INotificacionService notificacionService,
            ILogger<NotificacionesJob> logger)
        {
            _context = context;
            _emailService = emailService;
            _notificacionService = notificacionService;
            _logger = logger;
        }

        /// <summary>
        /// Verifica presupuestos cercanos al límite y envía alertas
        /// </summary>
        public async Task VerificarAlertasPresupuestosAsync()
        {
            _logger.LogInformation("Iniciando verificación de alertas de presupuestos...");

            var mesActual = DateTime.Now.Month;
            var anoActual = DateTime.Now.Year;

            var presupuestos = await _context.Presupuestos
                .Include(p => p.Categoria)
                .Include(p => p.User)
                .Where(p => p.MesAplicable == mesActual && p.AnoAplicable == anoActual)
                .ToListAsync();

            foreach (var presupuesto in presupuestos)
            {
                // Calcular gasto actual del mes
                var gastadoActual = await _context.Gastos
                    .Where(g => g.UserId == presupuesto.UserId
                               && g.CategoriaId == presupuesto.CategoriaId
                               && g.Fecha.Month == mesActual
                               && g.Fecha.Year == anoActual)
                    .SumAsync(g => (decimal?)g.Monto) ?? 0;

                var porcentaje = (gastadoActual / presupuesto.MontoLimite) * 100;

                // Alertar si supera el 80%
                if (porcentaje >= 80)
                {
                    var email = presupuesto.User?.Email;
                    if (!string.IsNullOrEmpty(email))
                    {
                        // Crear notificación
                        await _notificacionService.CrearNotificacionAsync(
                            presupuesto.UserId,
                            "PresupuestoAlerta",
                            $"⚠️ Alerta: Presupuesto {presupuesto.Categoria?.Nombre}",
                            $"Has utilizado {porcentaje:N1}% de tu presupuesto ({gastadoActual:C} de {presupuesto.MontoLimite:C})"
                        );

                        // Enviar email
                        await _emailService.SendAlertaPresupuestoAsync(
                            email,
                            presupuesto.Categoria?.Nombre ?? "Categoría",
                            gastadoActual,
                            presupuesto.MontoLimite,
                            porcentaje
                        );

                        _logger.LogInformation($"Alerta enviada para presupuesto {presupuesto.Id} - {porcentaje:N1}%");
                    }
                }
            }

            _logger.LogInformation("Verificación de presupuestos completada.");
        }

        /// <summary>
        /// Verifica metas próximas a vencer y envía recordatorios
        /// </summary>
        public async Task VerificarAlertasMetasAsync()
        {
            _logger.LogInformation("Iniciando verificación de alertas de metas...");

            var hoy = DateTime.Now.Date;
            var dentroDeUnaSemana = hoy.AddDays(7);

            // No hay FechaObjetivo en el modelo Meta actual, usando una alternativa
            // En una implementación completa, se agregaría esta propiedad al modelo
            _logger.LogInformation("Verificación de metas completada (modelo actual no tiene FechaObjetivo).");
        }

        /// <summary>
        /// Verifica gastos inusuales comparando contra el promedio por categoría
        /// </summary>
        public async Task VerificarGastosInusualesAsync()
        {
            _logger.LogInformation("Verificando gastos inusuales...");

            var configs = await _context.ConfiguracionesNotificaciones
                .Where(c => c.GastosInusuales)
                .ToListAsync();

            foreach (var config in configs)
            {
                var hace30Dias = DateTime.UtcNow.AddDays(-30);
                var hoy = DateTime.UtcNow.Date;

                // Promedios por categoría del último mes
                var promedios = await _context.Gastos
                    .Where(g => g.UserId == config.UserId && g.Fecha >= hace30Dias && g.Fecha < hoy)
                    .GroupBy(g => g.CategoriaId)
                    .Select(g => new { CategoriaId = g.Key, Promedio = g.Average(x => x.Monto) })
                    .ToListAsync();

                // Gastos de hoy
                var gastosHoy = await _context.Gastos
                    .Where(g => g.UserId == config.UserId && g.Fecha.Date == hoy)
                    .Include(g => g.Categoria)
                    .ToListAsync();

                foreach (var gasto in gastosHoy)
                {
                    var promedio = promedios.FirstOrDefault(p => p.CategoriaId == gasto.CategoriaId)?.Promedio ?? 0;
                    if (promedio > 0 && gasto.Monto > promedio * config.FactorGastoInusual)
                    {
                        await _notificacionService.CrearNotificacionAsync(
                            config.UserId,
                            "GastoInusual",
                            $"Gasto inusual en {gasto.Categoria?.Nombre}",
                            $"Registraste un gasto de {gasto.Monto:C} que es {(gasto.Monto / promedio):N1}x mayor al promedio ({promedio:C})."
                        );
                    }
                }
            }

            _logger.LogInformation("Verificación de gastos inusuales completada.");
        }

        /// <summary>
        /// Verifica pagos recurrentes próximos y envía recordatorios
        /// </summary>
        public async Task VerificarPagosRecurrentesAsync()
        {
            _logger.LogInformation("Verificando pagos recurrentes próximos...");

            var configs = await _context.ConfiguracionesNotificaciones
                .Where(c => c.AlertaPagoRecurrente)
                .ToListAsync();

            foreach (var config in configs)
            {
                var limite = DateTime.UtcNow.AddDays(config.DiasAntesPagoRecurrente);

                var gastosProximos = await _context.GastosRecurrentes
                    .Where(gr => gr.UserId == config.UserId && gr.Activo && gr.ProximaFecha <= limite && gr.ProximaFecha >= DateTime.UtcNow)
                    .Include(gr => gr.Categoria)
                    .ToListAsync();

                foreach (var gasto in gastosProximos)
                {
                    await _notificacionService.CrearNotificacionAsync(
                        config.UserId,
                        "Informativa",
                        $"Pago próximo: {gasto.Descripcion}",
                        $"Tu gasto recurrente de {gasto.Monto:C} ({gasto.Categoria?.Nombre}) vence el {gasto.ProximaFecha:dd/MM/yyyy}."
                    );
                }
            }

            _logger.LogInformation("Verificación de pagos recurrentes completada.");
        }

        /// <summary>
        /// Verifica si el balance de alguna cuenta está por debajo del umbral configurado
        /// </summary>
        public async Task VerificarBalanceBajoAsync()
        {
            _logger.LogInformation("Verificando balances bajos...");

            var configs = await _context.ConfiguracionesNotificaciones
                .Where(c => c.AlertaBalanceBajo && c.UmbralBalanceBajo > 0)
                .ToListAsync();

            foreach (var config in configs)
            {
                var cuentasBajas = await _context.Cuentas
                    .Where(c => c.UserId == config.UserId && c.Activa && c.BalanceActual < config.UmbralBalanceBajo)
                    .ToListAsync();

                foreach (var cuenta in cuentasBajas)
                {
                    await _notificacionService.CrearNotificacionAsync(
                        config.UserId,
                        "Informativa",
                        $"Balance bajo: {cuenta.Nombre}",
                        $"Tu cuenta '{cuenta.Nombre}' tiene un balance de {cuenta.BalanceActual:C}, por debajo del umbral de {config.UmbralBalanceBajo:C}."
                    );
                }
            }

            _logger.LogInformation("Verificación de balances bajos completada.");
        }

        /// <summary>
        /// Método principal que ejecuta todas las verificaciones
        /// </summary>
        public async Task EjecutarVerificacionesAsync()
        {
            _logger.LogInformation("=== Iniciando job de notificaciones ===");

            await VerificarAlertasPresupuestosAsync();
            await VerificarAlertasMetasAsync();
            await VerificarGastosInusualesAsync();
            await VerificarPagosRecurrentesAsync();
            await VerificarBalanceBajoAsync();

            _logger.LogInformation("=== Job de notificaciones completado ===");
        }
    }
}
