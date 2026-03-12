using FinanzasPersonales.Api.Data;
using FinanzasPersonales.Api.Services;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace FinanzasPersonales.Api.Jobs
{
    /// <summary>
    /// Job de Hangfire que envía reportes programados por email según la frecuencia configurada.
    /// </summary>
    public class ReportesProgramadosJob
    {
        private readonly FinanzasDbContext _context;
        private readonly IExportService _exportService;
        private readonly IEmailService _emailService;
        private readonly ILogger<ReportesProgramadosJob> _logger;

        public ReportesProgramadosJob(
            FinanzasDbContext context,
            IExportService exportService,
            IEmailService emailService,
            ILogger<ReportesProgramadosJob> logger)
        {
            _context = context;
            _exportService = exportService;
            _emailService = emailService;
            _logger = logger;
        }

        /// <summary>
        /// Ejecuta el envío de reportes programados pendientes.
        /// Se ejecuta diariamente y verifica qué reportes deben enviarse.
        /// </summary>
        public async Task EjecutarEnvioReportesAsync()
        {
            _logger.LogInformation("=== Iniciando job de reportes programados ===");

            var reportes = await _context.ReportesProgramados
                .Where(r => r.Activo)
                .ToListAsync();

            var ahora = DateTime.UtcNow;

            foreach (var reporte in reportes)
            {
                try
                {
                    bool debeEnviar = false;

                    if (reporte.UltimoEnvio == null)
                    {
                        debeEnviar = true;
                    }
                    else if (reporte.Frecuencia == "Semanal")
                    {
                        debeEnviar = (ahora - reporte.UltimoEnvio.Value).TotalDays >= 7;
                    }
                    else if (reporte.Frecuencia == "Mensual")
                    {
                        debeEnviar = (ahora - reporte.UltimoEnvio.Value).TotalDays >= 30;
                    }

                    if (!debeEnviar) continue;

                    // Determinar período del reporte
                    DateTime desde, hasta;
                    hasta = ahora;

                    if (reporte.Frecuencia == "Semanal")
                    {
                        desde = ahora.AddDays(-7);
                    }
                    else
                    {
                        desde = ahora.AddMonths(-1);
                    }

                    // Parsear secciones a incluir
                    var secciones = JsonSerializer.Deserialize<List<string>>(reporte.SeccionesIncluir) ?? new() { "gastos", "ingresos" };

                    // Generar PDF usando el ExportService existente
                    var pdfBytes = await _exportService.ExportToPdfAsync(reporte.UserId, desde, hasta, secciones);

                    // Enviar por email
                    var periodo = reporte.Frecuencia == "Semanal"
                        ? $"Semana del {desde:dd/MM/yyyy} al {hasta:dd/MM/yyyy}"
                        : $"Mes de {desde:MMMM yyyy}";

                    await _emailService.SendReportePdfAsync(reporte.EmailDestino, pdfBytes, periodo);

                    // Actualizar fecha de último envío
                    reporte.UltimoEnvio = ahora;
                    await _context.SaveChangesAsync();

                    _logger.LogInformation("Reporte programado {Id} enviado a {Email}", reporte.Id, reporte.EmailDestino);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error al enviar reporte programado {Id}", reporte.Id);
                }
            }

            _logger.LogInformation("=== Job de reportes programados completado ===");
        }
    }
}
