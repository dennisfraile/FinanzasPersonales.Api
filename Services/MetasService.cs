using FinanzasPersonales.Api.Data;
using Microsoft.EntityFrameworkCore;

namespace FinanzasPersonales.Api.Services
{
    /// <summary>
    /// Implementación del servicio de metas mejoradas.
    /// </summary>
    public class MetasService : IMetasService
    {
        private readonly FinanzasDbContext _context;

        public MetasService(FinanzasDbContext context)
        {
            _context = context;
        }

        public async Task<object> CalcularProgresoAsync(int metaId, string userId)
        {
            var meta = await _context.Metas
                .FirstOrDefaultAsync(m => m.Id == metaId && m.UserId == userId);

            if (meta == null)
                throw new KeyNotFoundException("Meta no encontrada");

            var porcentaje = meta.MontoTotal > 0 ? (meta.AhorroActual / meta.MontoTotal) * 100 : 0;
            var faltante = meta.MontoTotal - meta.AhorroActual;

            return new
            {
                MetaId = meta.Id,
                Nombre = meta.Metas,
                MontoTotal = meta.MontoTotal,
                AhorroActual = meta.AhorroActual,
                MontoRestante = meta.MontoRestante,
                PorcentajeProgreso = Math.Round(porcentaje, 2),
                Estado = porcentaje >= 100 ? "Completada" :
                         porcentaje >= 75 ? "Casi completada" :
                         porcentaje >= 50 ? "En progreso" : "Iniciada",
                FaltanteParaCompletarr = faltante > 0 ? faltante : 0
            };
        }

        public async Task<bool> AbonarMetaAsync(int metaId, string userId, decimal monto)
        {
            if (monto <= 0)
                throw new ArgumentException("El monto debe ser mayor a 0");

            var meta = await _context.Metas
                .FirstOrDefaultAsync(m => m.Id == metaId && m.UserId == userId);

            if (meta == null)
                return false;

            meta.AhorroActual += monto;
            meta.MontoRestante = meta.MontoTotal - meta.AhorroActual;

            if (meta.MontoRestante < 0)
                meta.MontoRestante = 0;

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<List<object>> ObtenerProyeccionesAsync(string userId)
        {
            var metas = await _context.Metas
                .Where(m => m.UserId == userId)
                .ToListAsync();

            var proyecciones = new List<object>();

            foreach (var meta in metas)
            {
                var porcentajeActual = meta.MontoTotal > 0 ? (meta.AhorroActual / meta.MontoTotal) * 100 : 0;
                var faltante = meta.MontoTotal - meta.AhorroActual;

                // Proyección simple: calcular cuánto falta ahorrar
                var proyeccion = new
                {
                    MetaId = meta.Id,
                    Nombre = meta.Metas,
                    MontoTotal = meta.MontoTotal,
                    AhorroActual = meta.AhorroActual,
                    PorcentajeProgreso = Math.Round(porcentajeActual, 2),
                    FaltanteMonto = faltante > 0 ? faltante : 0,
                    Estado = porcentajeActual >= 100 ? "Completada" : "En progreso",

                    // Proyecciones basadas en diferentes escenarios
                    Escenarios = new
                    {
                        // Si ahorro $100 mensuales
                        AhorroMensual100 = faltante > 0 ? Math.Ceiling(faltante / 100) : 0,
                        // Si ahorro $200 mensuales
                        AhorroMensual200 = faltante > 0 ? Math.Ceiling(faltante / 200) : 0,
                        // Si ahorro $500 mensuales
                        AhorroMensual500 = faltante > 0 ? Math.Ceiling(faltante / 500) : 0,
                    }
                };

                proyecciones.Add(proyeccion);
            }

            return proyecciones;
        }
    }
}
