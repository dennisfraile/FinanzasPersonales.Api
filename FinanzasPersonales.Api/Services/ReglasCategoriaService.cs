using Microsoft.EntityFrameworkCore;
using FinanzasPersonales.Api.Data;
using FinanzasPersonales.Api.Dtos;
using FinanzasPersonales.Api.Models;

namespace FinanzasPersonales.Api.Services
{
    public interface IReglasCategoriaService
    {
        Task<List<ReglaCategoriaDto>> GetReglasAsync(string userId);
        Task<ReglaCategoriaDto> CreateReglaAsync(string userId, CreateReglaCategoriaDto dto);
        Task<bool> UpdateReglaAsync(string userId, int id, UpdateReglaCategoriaDto dto);
        Task<bool> DeleteReglaAsync(string userId, int id);
        Task<CategoriaSugeridaDto?> SugerirCategoriaAsync(string userId, string descripcion, string tipoTransaccion);
    }

    public class ReglasCategoriaService : IReglasCategoriaService
    {
        private readonly FinanzasDbContext _context;

        public ReglasCategoriaService(FinanzasDbContext context)
        {
            _context = context;
        }

        public async Task<List<ReglaCategoriaDto>> GetReglasAsync(string userId)
        {
            return await _context.ReglasCategoriaAutomatica
                .Where(r => r.UserId == userId)
                .Include(r => r.Categoria)
                .OrderByDescending(r => r.Prioridad)
                .Select(r => new ReglaCategoriaDto
                {
                    Id = r.Id,
                    Patron = r.Patron,
                    TipoCoincidencia = r.TipoCoincidencia,
                    CategoriaId = r.CategoriaId,
                    CategoriaNombre = r.Categoria != null ? r.Categoria.Nombre : "",
                    TipoTransaccion = r.TipoTransaccion,
                    Prioridad = r.Prioridad,
                    Activa = r.Activa
                })
                .ToListAsync();
        }

        public async Task<ReglaCategoriaDto> CreateReglaAsync(string userId, CreateReglaCategoriaDto dto)
        {
            var categoriaExiste = await _context.Categorias
                .AnyAsync(c => c.Id == dto.CategoriaId && c.UserId == userId);
            if (!categoriaExiste)
                throw new InvalidOperationException("Recurso no encontrado o acceso denegado.");

            var regla = new ReglaCategoriaAutomatica
            {
                UserId = userId,
                Patron = dto.Patron,
                TipoCoincidencia = dto.TipoCoincidencia,
                CategoriaId = dto.CategoriaId,
                TipoTransaccion = dto.TipoTransaccion,
                Prioridad = dto.Prioridad
            };

            _context.ReglasCategoriaAutomatica.Add(regla);
            await _context.SaveChangesAsync();

            var categoria = await _context.Categorias.FindAsync(dto.CategoriaId);

            return new ReglaCategoriaDto
            {
                Id = regla.Id,
                Patron = regla.Patron,
                TipoCoincidencia = regla.TipoCoincidencia,
                CategoriaId = regla.CategoriaId,
                CategoriaNombre = categoria?.Nombre ?? "",
                TipoTransaccion = regla.TipoTransaccion,
                Prioridad = regla.Prioridad,
                Activa = regla.Activa
            };
        }

        public async Task<bool> UpdateReglaAsync(string userId, int id, UpdateReglaCategoriaDto dto)
        {
            var regla = await _context.ReglasCategoriaAutomatica
                .FirstOrDefaultAsync(r => r.Id == id && r.UserId == userId);

            if (regla == null) return false;

            var categoriaExiste = await _context.Categorias
                .AnyAsync(c => c.Id == dto.CategoriaId && c.UserId == userId);
            if (!categoriaExiste)
                throw new InvalidOperationException("Recurso no encontrado o acceso denegado.");

            regla.Patron = dto.Patron;
            regla.TipoCoincidencia = dto.TipoCoincidencia;
            regla.CategoriaId = dto.CategoriaId;
            regla.TipoTransaccion = dto.TipoTransaccion;
            regla.Prioridad = dto.Prioridad;
            regla.Activa = dto.Activa;

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeleteReglaAsync(string userId, int id)
        {
            var regla = await _context.ReglasCategoriaAutomatica
                .FirstOrDefaultAsync(r => r.Id == id && r.UserId == userId);

            if (regla == null) return false;

            _context.ReglasCategoriaAutomatica.Remove(regla);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<CategoriaSugeridaDto?> SugerirCategoriaAsync(string userId, string descripcion, string tipoTransaccion)
        {
            if (string.IsNullOrWhiteSpace(descripcion))
                return null;

            var reglas = await _context.ReglasCategoriaAutomatica
                .Where(r => r.UserId == userId && r.Activa &&
                    (r.TipoTransaccion == tipoTransaccion || r.TipoTransaccion == "Ambos"))
                .Include(r => r.Categoria)
                .OrderByDescending(r => r.Prioridad)
                .ToListAsync();

            var descripcionLower = descripcion.ToLower();

            foreach (var regla in reglas)
            {
                var patronLower = regla.Patron.ToLower();
                bool coincide = regla.TipoCoincidencia switch
                {
                    "Contiene" => descripcionLower.Contains(patronLower),
                    "Exacto" => descripcionLower == patronLower,
                    "ComienzaCon" => descripcionLower.StartsWith(patronLower),
                    _ => false
                };

                if (coincide)
                {
                    return new CategoriaSugeridaDto
                    {
                        CategoriaId = regla.CategoriaId,
                        CategoriaNombre = regla.Categoria?.Nombre ?? "",
                        ReglaId = regla.Id,
                        PatronCoincidido = regla.Patron
                    };
                }
            }

            return null;
        }
    }
}
