namespace FinanzasPersonales.Api.Dtos
{
    /// <summary>
    /// DTO para lectura de notificaciones
    /// </summary>
    public class NotificacionDto
    {
        public int Id { get; set; }
        public string Tipo { get; set; } = string.Empty;
        public string Titulo { get; set; } = string.Empty;
        public string Mensaje { get; set; } = string.Empty;
        public DateTime FechaCreacion { get; set; }
        public bool Leida { get; set; }
        public bool EmailEnviado { get; set; }
        public int? ReferenciaId { get; set; }
    }

    /// <summary>
    /// DTO para configuración de notificaciones
    /// </summary>
    public class ConfiguracionNotificacionesDto
    {
        public bool AlertasPresupuesto { get; set; }
        public int UmbralPresupuesto { get; set; }
        public bool AlertasMetas { get; set; }
        public int DiasAntesMeta { get; set; }
        public string? Email { get; set; }
        public bool RecordatorioMetas { get; set; }
        public bool GastosInusuales { get; set; }
        public bool ResumenMensual { get; set; }
        public int UmbralMeta { get; set; }
        public decimal FactorGastoInusual { get; set; }
    }
}
