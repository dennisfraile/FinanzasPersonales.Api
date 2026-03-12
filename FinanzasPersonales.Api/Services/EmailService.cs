using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;

namespace FinanzasPersonales.Api.Services
{
    /// <summary>
    /// Implementación del servicio de envío de emails usando MailKit.
    /// </summary>
    public class EmailService : IEmailService
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<EmailService> _logger;

        public EmailService(IConfiguration configuration, ILogger<EmailService> logger)
        {
            _configuration = configuration;
            _logger = logger;
        }

        public async Task SendAlertaPresupuestoAsync(string email, string categoriaNombre, decimal gastado, decimal limite, decimal porcentaje)
        {
            var subject = $"⚠️ Alerta de Presupuesto: {categoriaNombre}";
            var body = $@"
                <h2>Alerta de Presupuesto</h2>
                <p>Tu presupuesto para <strong>{categoriaNombre}</strong> está cerca del límite.</p>
                <ul>
                    <li>Gastado: <strong>${gastado:N2}</strong></li>
                    <li>Límite: <strong>${limite:N2}</strong></li>
                    <li>Porcentaje utilizado: <strong>{porcentaje:N1}%</strong></li>
                </ul>
                <p>Te recomendamos revisar tus gastos para mantenerte dentro del presupuesto.</p>
            ";

            await SendEmailAsync(email, subject, body);
        }

        public async Task SendRecordatorioMetaAsync(string email, string metaNombre, DateTime fechaObjetivo, int diasRestantes)
        {
            var subject = $"🎯 Recordatorio: Meta '{metaNombre}' próxima a vencer";
            var body = $@"
                <h2>Recordatorio de Meta</h2>
                <p>Tu meta <strong>{metaNombre}</strong> está próxima a su fecha objetivo.</p>
                <ul>
                    <li>Fecha objetivo: <strong>{fechaObjetivo:dd/MM/yyyy}</strong></li>
                    <li>Días restantes: <strong>{diasRestantes}</strong></li>
                </ul>
                <p>¡Mantén el enfoque para alcanzar tu meta!</p>
            ";

            await SendEmailAsync(email, subject, body);
        }

        public async Task SendMetaCumplidaAsync(string email, string metaNombre, decimal montoTotal)
        {
            var subject = $"🎉 ¡Felicitaciones! Meta '{metaNombre}' cumplida";
            var body = $@"
                <h2>¡Meta Cumplida!</h2>
                <p>¡Felicitaciones! Has alcanzado tu meta <strong>{metaNombre}</strong>.</p>
                <ul>
                    <li>Monto objetivo: <strong>${montoTotal:N2}</strong></li>
                </ul>
                <p>¡Excelente trabajo! Sigue así con tus finanzas personales.</p>
            ";

            await SendEmailAsync(email, subject, body);
        }

        public async Task SendReportePdfAsync(string email, byte[] pdfBytes, string periodo)
        {
            var subject = $"Reporte Financiero - {periodo}";
            var body = $@"
                <h2>Reporte Financiero</h2>
                <p>Adjunto encontrarás tu reporte financiero correspondiente al período: <strong>{periodo}</strong>.</p>
                <p>Este reporte fue generado automáticamente según tu configuración de reportes programados.</p>
            ";

            await SendEmailWithAttachmentAsync(email, subject, body, pdfBytes, $"Reporte_{DateTime.UtcNow:yyyyMMdd}.pdf", "application/pdf");
        }

        private async Task SendEmailWithAttachmentAsync(string toEmail, string subject, string htmlBody, byte[] attachmentBytes, string attachmentName, string contentType)
        {
            try
            {
                var smtpServer = _configuration["EmailSettings:SmtpServer"];
                var smtpPort = int.Parse(_configuration["EmailSettings:SmtpPort"] ?? "587");
                var senderEmail = _configuration["EmailSettings:SenderEmail"];
                var senderName = _configuration["EmailSettings:SenderName"];
                var username = _configuration["EmailSettings:Username"];
                var password = _configuration["EmailSettings:Password"];
                var enableSsl = bool.Parse(_configuration["EmailSettings:EnableSsl"] ?? "true");

                var message = new MimeMessage();
                message.From.Add(new MailboxAddress(senderName, senderEmail));
                message.To.Add(new MailboxAddress("", toEmail));
                message.Subject = subject;

                var bodyBuilder = new BodyBuilder
                {
                    HtmlBody = htmlBody
                };
                bodyBuilder.Attachments.Add(attachmentName, attachmentBytes, new MimeKit.ContentType(contentType.Split('/')[0], contentType.Split('/')[1]));
                message.Body = bodyBuilder.ToMessageBody();

                using var client = new SmtpClient();
                await client.ConnectAsync(smtpServer, smtpPort, enableSsl ? SecureSocketOptions.StartTls : SecureSocketOptions.None);

                if (!string.IsNullOrEmpty(username) && !string.IsNullOrEmpty(password))
                {
                    await client.AuthenticateAsync(username, password);
                }

                await client.SendAsync(message);
                await client.DisconnectAsync(true);

                _logger.LogInformation($"Email con adjunto enviado exitosamente a {toEmail}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error al enviar email con adjunto a {toEmail}: {ex.Message}");
            }
        }

        private async Task SendEmailAsync(string toEmail, string subject, string htmlBody)
        {
            try
            {
                var smtpServer = _configuration["EmailSettings:SmtpServer"];
                var smtpPort = int.Parse(_configuration["EmailSettings:SmtpPort"] ?? "587");
                var senderEmail = _configuration["EmailSettings:SenderEmail"];
                var senderName = _configuration["EmailSettings:SenderName"];
                var username = _configuration["EmailSettings:Username"];
                var password = _configuration["EmailSettings:Password"];
                var enableSsl = bool.Parse(_configuration["EmailSettings:EnableSsl"] ?? "true");

                var message = new MimeMessage();
                message.From.Add(new MailboxAddress(senderName, senderEmail));
                message.To.Add(new MailboxAddress("", toEmail));
                message.Subject = subject;

                var bodyBuilder = new BodyBuilder
                {
                    HtmlBody = htmlBody
                };
                message.Body = bodyBuilder.ToMessageBody();

                using var client = new SmtpClient();
                await client.ConnectAsync(smtpServer, smtpPort, enableSsl ? SecureSocketOptions.StartTls : SecureSocketOptions.None);

                if (!string.IsNullOrEmpty(username) && !string.IsNullOrEmpty(password))
                {
                    await client.AuthenticateAsync(username, password);
                }

                await client.SendAsync(message);
                await client.DisconnectAsync(true);

                _logger.LogInformation($"Email enviado exitosamente a {toEmail}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error al enviar email a {toEmail}: {ex.Message}");
                // No lanzamos excepción para que no falle el proceso principal si el email falla
            }
        }
    }
}
