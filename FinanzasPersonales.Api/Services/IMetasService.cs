namespace FinanzasPersonales.Api.Services
{
    /// <summary>
    /// Servicio para gestión avanzada de metas financieras.
    /// </summary>
    public interface IMetasService
    {
        /// <summary>
        /// Calcula el progreso detallado de una meta
        /// </summary>
        Task<object> CalcularProgresoAsync(int metaId, string userId);

        /// <summary>
        /// Registra un abono manual a una meta
        /// </summary>
        Task<bool> AbonarMetaAsync(int metaId, string userId, decimal monto);

        /// <summary>
        /// Obtiene proyecciones de cumplimiento de metas
        /// </summary>
        Task<List<object>> ObtenerProyeccionesAsync(string userId);

        /// <summary>
        /// Genera abonos automáticos pendientes para un usuario
        /// </summary>
        Task<int> GenerarAbonosAutomaticosAsync(string userId);
    }
}
