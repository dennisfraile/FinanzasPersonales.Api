using System.ComponentModel.DataAnnotations;

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

        [Range(50, 100, ErrorMessage = "El umbral de presupuesto debe estar entre 50 y 100")]
        public int UmbralPresupuesto { get; set; }

        public bool AlertasMetas { get; set; }

        [Range(1, 365, ErrorMessage = "Los días antes de meta deben estar entre 1 y 365")]
        public int DiasAntesMeta { get; set; }

        [EmailAddress(ErrorMessage = "El formato del email no es válido")]
        public string? Email { get; set; }

        public bool RecordatorioMetas { get; set; }
        public bool GastosInusuales { get; set; }
        public bool ResumenMensual { get; set; }

        [Range(50, 100, ErrorMessage = "El umbral de meta debe estar entre 50 y 100")]
        public int UmbralMeta { get; set; }

        [Range(1.5, 5.0, ErrorMessage = "El factor de gasto inusual debe estar entre 1.5 y 5.0")]
        public decimal FactorGastoInusual { get; set; }
    }
}
