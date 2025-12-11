using System.ComponentModel.DataAnnotations;

namespace FinanzasPersonales.Api.Dtos
{
    /// <summary>
    /// DTO para configuración de notificaciones del usuario.
    /// </summary>
    public class ConfiguracionNotificacionesDto
    {
        /// <summary>
        /// Recibir alertas cuando presupuesto supera umbral
        /// </summary>
        public bool AlertasPresupuesto { get; set; } = true;

        /// <summary>
        /// Porcentaje de presupuesto para activar alerta (ej: 80, 90)
        /// </summary>
        [Range(50, 100)]
        public int UmbralPresupuesto { get; set; } = 80;

        /// <summary>
        /// Recibir alertas de metas próximas a vencer
        /// </summary>
        public bool AlertasMetas { get; set; } = true;

        /// <summary>
        /// Días antes del vencimiento para enviar alerta
        /// </summary>
        [Range(1, 30)]
        public int DiasAntesMeta { get; set; } = 7;

        /// <summary>
        /// Email del usuario para notificaciones
        /// </summary>
        [EmailAddress]
        public string? Email { get; set; }
    }
}
