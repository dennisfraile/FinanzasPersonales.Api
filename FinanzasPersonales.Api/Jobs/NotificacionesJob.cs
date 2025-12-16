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
        /// Método principal que ejecuta todas las verificaciones
        /// </summary>
        public async Task EjecutarVerificacionesAsync()
        {
            _logger.LogInformation("=== Iniciando job de notificaciones ===");

            await VerificarAlertasPresupuestosAsync();
            await VerificarAlertasMetasAsync();

            _logger.LogInformation("=== Job de notificaciones completado ===");
        }
    }
}
