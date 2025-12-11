namespace FinanzasPersonales.Api.Dtos
{
    /// <summary>
    /// DTO para lectura de notificaciones.
    /// </summary>
    public class NotificacionDto
    {
        /// <summary>
        /// Identificador único de la notificación.
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Tipo de notificación (ej: Alerta, Recordatorio, etc.).
        /// </summary>
        public required string Tipo { get; set; }

        /// <summary>
        /// Título de la notificación.
        /// </summary>
        public required string Titulo { get; set; }

        /// <summary>
        /// Mensaje o contenido de la notificación.
        /// </summary>
        public required string Mensaje { get; set; }

        /// <summary>
        /// Fecha y hora de creación de la notificación.
        /// </summary>
        public DateTime FechaCreacion { get; set; }

        /// <summary>
        /// Indica si la notificación ha sido leída.
        /// </summary>
        public bool Leida { get; set; }

        /// <summary>
        /// Indica si se ha enviado un email con esta notificación.
        /// </summary>
        public bool EmailEnviado { get; set; }
    }
}
