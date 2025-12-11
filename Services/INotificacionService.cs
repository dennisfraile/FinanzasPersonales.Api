using FinanzasPersonales.Api.Dtos;

namespace FinanzasPersonales.Api.Services
{
    /// <summary>
    /// Servicio para gestión de notificaciones.
    /// </summary>
    public interface INotificacionService
    {
        /// <summary>
        /// Crea una nueva notificación en la base de datos
        /// </summary>
        Task<int> CrearNotificacionAsync(string userId, string tipo, string titulo, string mensaje);

        /// <summary>
        /// Marca una notificación como leída
        /// </summary>
        Task MarcarComoLeidaAsync(int notificacionId, string userId);

        /// <summary>
        /// Obtiene notificaciones no leídas del usuario
        /// </summary>
        Task<List<NotificacionDto>> ObtenerNoLeidasAsync(string userId);

        /// <summary>
        /// Obtiene todas las notificaciones del usuario
        /// </summary>
        Task<List<NotificacionDto>> ObtenerTodasAsync(string userId, bool soloNoLeidas = false);
    }
}
