namespace FinanzasPersonales.Api.Services
{
    /// <summary>
    /// Servicio para envío de emails.
    /// </summary>
    public interface IEmailService
    {
        /// <summary>
        /// Envía alerta cuando un presupuesto supera el umbral configurado
        /// </summary>
        Task SendAlertaPresupuestoAsync(string email, string categoriaNombre, decimal gastado, decimal limite, decimal porcentaje);

        /// <summary>
        /// Envía recordatorio de meta próxima a vencer
        /// </summary>
        Task SendRecordatorioMetaAsync(string email, string metaNombre, DateTime fechaObjetivo, int diasRestantes);

        /// <summary>
        /// Envía felicitación por meta cumplida
        /// </summary>
        Task SendMetaCumplidaAsync(string email, string metaNombre, decimal montoTotal);
    }
}
