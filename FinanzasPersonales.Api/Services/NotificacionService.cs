using FinanzasPersonales.Api.Data;
using FinanzasPersonales.Api.Dtos;
using FinanzasPersonales.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace FinanzasPersonales.Api.Services
{
    /// <summary>
    /// Implementación del servicio de notificaciones.
    /// </summary>
    public class NotificacionService : INotificacionService
    {
        private readonly FinanzasDbContext _context;

        public NotificacionService(FinanzasDbContext context)
        {
            _context = context;
        }

        public async Task<int> CrearNotificacionAsync(string userId, string tipo, string titulo, string mensaje)
        {
            var notificacion = new Notificacion
            {
                UserId = userId,
                Tipo = Enum.Parse<TipoNotificacion>(tipo),
                Titulo = titulo,
                Mensaje = mensaje,
                FechaCreacion = DateTime.Now,
                Leida = false,
                EmailEnviado = false
            };

            _context.Notificaciones.Add(notificacion);
            await _context.SaveChangesAsync();

            return notificacion.Id;
        }

        public async Task MarcarComoLeidaAsync(int notificacionId, string userId)
        {
            var notificacion = await _context.Notificaciones
                .FirstOrDefaultAsync(n => n.Id == notificacionId && n.UserId == userId);

            if (notificacion != null)
            {
                notificacion.Leida = true;
                await _context.SaveChangesAsync();
            }
        }

        public async Task<List<NotificacionDto>> ObtenerNoLeidasAsync(string userId)
        {
            return await _context.Notificaciones
                .Where(n => n.UserId == userId && !n.Leida)
                .OrderByDescending(n => n.FechaCreacion)
                .Select(n => new NotificacionDto
                {
                    Id = n.Id,
                    Tipo = n.Tipo.ToString(),
                    Titulo = n.Titulo,
                    Mensaje = n.Mensaje,
                    FechaCreacion = n.FechaCreacion,
                    Leida = n.Leida,
                    EmailEnviado = n.EmailEnviado
                })
                .ToListAsync();
        }

        public async Task<List<NotificacionDto>> ObtenerTodasAsync(string userId, bool soloNoLeidas = false)
        {
            var query = _context.Notificaciones.Where(n => n.UserId == userId);

            if (soloNoLeidas)
            {
                query = query.Where(n => !n.Leida);
            }

            return await query
                .OrderByDescending(n => n.FechaCreacion)
                .Select(n => new NotificacionDto
                {
                    Id = n.Id,
                    Tipo = n.Tipo.ToString(),
                    Titulo = n.Titulo,
                    Mensaje = n.Mensaje,
                    FechaCreacion = n.FechaCreacion,
                    Leida = n.Leida,
                    EmailEnviado = n.EmailEnviado
                })
                .ToListAsync();
        }
    }
}
