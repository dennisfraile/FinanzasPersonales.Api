using Microsoft.EntityFrameworkCore;
using FinanzasPersonales.Api.Data;
using FinanzasPersonales.Api.Dtos;
using FinanzasPersonales.Api.Models;

namespace FinanzasPersonales.Api.Services
{
    public interface IPlantillasGastoService
    {
        Task<List<PlantillaGastoDto>> GetPlantillasAsync(string userId);
        Task<PlantillaGastoDto> CreatePlantillaAsync(string userId, CreatePlantillaGastoDto dto);
        Task<bool> UpdatePlantillaAsync(string userId, int id, UpdatePlantillaGastoDto dto);
        Task<bool> DeletePlantillaAsync(string userId, int id);
        Task<GastoDto> UsarPlantillaAsync(string userId, int plantillaId, UsarPlantillaDto dto);
    }

    public class PlantillasGastoService : IPlantillasGastoService
    {
        private readonly FinanzasDbContext _context;
        private readonly IGastosService _gastosService;

        public PlantillasGastoService(FinanzasDbContext context, IGastosService gastosService)
        {
            _context = context;
            _gastosService = gastosService;
        }

        public async Task<List<PlantillaGastoDto>> GetPlantillasAsync(string userId)
        {
            return await _context.PlantillasGasto
                .Where(p => p.UserId == userId)
                .Include(p => p.Categoria)
                .OrderBy(p => p.OrdenDisplay)
                .ThenByDescending(p => p.VecesUsada)
                .Select(p => new PlantillaGastoDto
                {
                    Id = p.Id,
                    Nombre = p.Nombre,
                    CategoriaId = p.CategoriaId,
                    CategoriaNombre = p.Categoria != null ? p.Categoria.Nombre : "",
                    Monto = p.Monto,
                    Descripcion = p.Descripcion,
                    Tipo = p.Tipo,
                    CuentaId = p.CuentaId,
                    Icono = p.Icono,
                    Color = p.Color,
                    OrdenDisplay = p.OrdenDisplay,
                    VecesUsada = p.VecesUsada
                })
                .ToListAsync();
        }

        public async Task<PlantillaGastoDto> CreatePlantillaAsync(string userId, CreatePlantillaGastoDto dto)
        {
            var categoriaExiste = await _context.Categorias
                .AnyAsync(c => c.Id == dto.CategoriaId && c.UserId == userId);
            if (!categoriaExiste)
                throw new InvalidOperationException("Recurso no encontrado o acceso denegado.");

            var plantilla = new PlantillaGasto
            {
                UserId = userId,
                Nombre = dto.Nombre,
                CategoriaId = dto.CategoriaId,
                Monto = dto.Monto,
                Descripcion = dto.Descripcion,
                Tipo = dto.Tipo,
                CuentaId = dto.CuentaId,
                Icono = dto.Icono,
                Color = dto.Color,
                OrdenDisplay = dto.OrdenDisplay
            };

            _context.PlantillasGasto.Add(plantilla);
            await _context.SaveChangesAsync();

            var categoria = await _context.Categorias.FindAsync(dto.CategoriaId);

            return new PlantillaGastoDto
            {
                Id = plantilla.Id,
                Nombre = plantilla.Nombre,
                CategoriaId = plantilla.CategoriaId,
                CategoriaNombre = categoria?.Nombre ?? "",
                Monto = plantilla.Monto,
                Descripcion = plantilla.Descripcion,
                Tipo = plantilla.Tipo,
                CuentaId = plantilla.CuentaId,
                Icono = plantilla.Icono,
                Color = plantilla.Color,
                OrdenDisplay = plantilla.OrdenDisplay,
                VecesUsada = 0
            };
        }

        public async Task<bool> UpdatePlantillaAsync(string userId, int id, UpdatePlantillaGastoDto dto)
        {
            var plantilla = await _context.PlantillasGasto
                .FirstOrDefaultAsync(p => p.Id == id && p.UserId == userId);

            if (plantilla == null) return false;

            var categoriaExiste = await _context.Categorias
                .AnyAsync(c => c.Id == dto.CategoriaId && c.UserId == userId);
            if (!categoriaExiste)
                throw new InvalidOperationException("Recurso no encontrado o acceso denegado.");

            plantilla.Nombre = dto.Nombre;
            plantilla.CategoriaId = dto.CategoriaId;
            plantilla.Monto = dto.Monto;
            plantilla.Descripcion = dto.Descripcion;
            plantilla.Tipo = dto.Tipo;
            plantilla.CuentaId = dto.CuentaId;
            plantilla.Icono = dto.Icono;
            plantilla.Color = dto.Color;
            plantilla.OrdenDisplay = dto.OrdenDisplay;

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeletePlantillaAsync(string userId, int id)
        {
            var plantilla = await _context.PlantillasGasto
                .FirstOrDefaultAsync(p => p.Id == id && p.UserId == userId);

            if (plantilla == null) return false;

            _context.PlantillasGasto.Remove(plantilla);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<GastoDto> UsarPlantillaAsync(string userId, int plantillaId, UsarPlantillaDto dto)
        {
            var plantilla = await _context.PlantillasGasto
                .FirstOrDefaultAsync(p => p.Id == plantillaId && p.UserId == userId);

            if (plantilla == null)
                throw new InvalidOperationException("Recurso no encontrado o acceso denegado.");

            var monto = dto.Monto ?? plantilla.Monto;
            if (!monto.HasValue || monto.Value <= 0)
                throw new InvalidOperationException("Debe proporcionar un monto válido.");

            var createDto = new CreateGastoDto
            {
                Fecha = dto.Fecha ?? DateTime.UtcNow,
                CategoriaId = plantilla.CategoriaId,
                Tipo = plantilla.Tipo,
                Descripcion = plantilla.Descripcion,
                Monto = monto.Value,
                CuentaId = plantilla.CuentaId
            };

            var gasto = await _gastosService.CreateGastoAsync(userId, createDto);

            plantilla.VecesUsada++;
            await _context.SaveChangesAsync();

            return new GastoDto
            {
                Id = gasto.Id,
                Fecha = gasto.Fecha,
                CategoriaId = gasto.CategoriaId,
                Tipo = gasto.Tipo,
                Descripcion = gasto.Descripcion,
                Monto = gasto.Monto
            };
        }
    }
}
