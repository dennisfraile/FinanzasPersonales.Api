namespace FinanzasPersonales.Api.Services
{
    /// <summary>
    /// Servicio para exportación de datos en diferentes formatos.
    /// </summary>
    public interface IExportService
    {
        /// <summary>
        /// Exporta datos del usuario a formato Excel (.xlsx)
        /// </summary>
        Task<byte[]> ExportToExcelAsync(string userId, DateTime desde, DateTime hasta, List<string> incluir);

        /// <summary>
        /// Exporta datos del usuario a formato PDF
        /// </summary>
        Task<byte[]> ExportToPdfAsync(string userId, DateTime desde, DateTime hasta, List<string> incluir);

        /// <summary>
        /// Exporta backup completo de datos del usuario en formato JSON
        /// </summary>
        Task<string> ExportToJsonAsync(string userId, DateTime? desde = null, DateTime? hasta = null);
    }
}
