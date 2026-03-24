using Microsoft.EntityFrameworkCore;
using FinanzasPersonales.Api.Data;
using FinanzasPersonales.Api.Dtos;
using FinanzasPersonales.Api.Models;

namespace FinanzasPersonales.Api.Services
{
    public class PlantillasIngresoService : IPlantillasIngresoService
    {
        private readonly FinanzasDbContext _context;

        public PlantillasIngresoService(FinanzasDbContext context)
        {
            _context = context;
        }

        public async Task<List<PlantillaIngresoDto>> GetPlantillasAsync(string userId)
        {
            return await _context.PlantillasIngreso
                .Where(p => p.UserId == userId)
                .Include(p => p.Categoria)
                .OrderBy(p => p.OrdenDisplay)
                .ThenByDescending(p => p.VecesUsada)
                .Select(p => new PlantillaIngresoDto
                {
                    Id = p.Id,
                    Nombre = p.Nombre,
                    CategoriaId = p.CategoriaId,
                    CategoriaNombre = p.Categoria != null ? p.Categoria.Nombre : "",
                    Monto = p.Monto,
                    Descripcion = p.Descripcion,
                    CuentaId = p.CuentaId,
                    Icono = p.Icono,
                    Color = p.Color,
                    OrdenDisplay = p.OrdenDisplay,
                    VecesUsada = p.VecesUsada
                })
                .ToListAsync();
        }

        public async Task<PlantillaIngresoDto> CreatePlantillaAsync(string userId, CreatePlantillaIngresoDto dto)
        {
            var categoriaExiste = await _context.Categorias
                .AnyAsync(c => c.Id == dto.CategoriaId && c.UserId == userId);
            if (!categoriaExiste)
                throw new InvalidOperationException("Recurso no encontrado o acceso denegado.");

            var plantilla = new PlantillaIngreso
            {
                UserId = userId,
                Nombre = dto.Nombre,
                CategoriaId = dto.CategoriaId,
                Monto = dto.Monto,
                Descripcion = dto.Descripcion,
                CuentaId = dto.CuentaId,
                Icono = dto.Icono,
                Color = dto.Color,
                OrdenDisplay = dto.OrdenDisplay
            };

            _context.PlantillasIngreso.Add(plantilla);
            await _context.SaveChangesAsync();

            var categoria = await _context.Categorias.FindAsync(dto.CategoriaId);

            return new PlantillaIngresoDto
            {
                Id = plantilla.Id,
                Nombre = plantilla.Nombre,
                CategoriaId = plantilla.CategoriaId,
                CategoriaNombre = categoria?.Nombre ?? "",
                Monto = plantilla.Monto,
                Descripcion = plantilla.Descripcion,
                CuentaId = plantilla.CuentaId,
                Icono = plantilla.Icono,
                Color = plantilla.Color,
                OrdenDisplay = plantilla.OrdenDisplay,
                VecesUsada = 0
            };
        }

        public async Task<bool> UpdatePlantillaAsync(string userId, int id, UpdatePlantillaIngresoDto dto)
        {
            var plantilla = await _context.PlantillasIngreso
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
            plantilla.CuentaId = dto.CuentaId;
            plantilla.Icono = dto.Icono;
            plantilla.Color = dto.Color;
            plantilla.OrdenDisplay = dto.OrdenDisplay;

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeletePlantillaAsync(string userId, int id)
        {
            var plantilla = await _context.PlantillasIngreso
                .FirstOrDefaultAsync(p => p.Id == id && p.UserId == userId);

            if (plantilla == null) return false;

            _context.PlantillasIngreso.Remove(plantilla);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<object> UsarPlantillaAsync(string userId, int plantillaId, UsarPlantillaIngresoDto dto)
        {
            var plantilla = await _context.PlantillasIngreso
                .Include(p => p.Cuenta)
                .FirstOrDefaultAsync(p => p.Id == plantillaId && p.UserId == userId);

            if (plantilla == null)
                throw new InvalidOperationException("Recurso no encontrado o acceso denegado.");

            var montoFinal = dto.Monto ?? plantilla.Monto;
            if (!montoFinal.HasValue || montoFinal.Value <= 0)
                throw new InvalidOperationException("Debe proporcionar un monto válido.");

            var fechaFinal = dto.Fecha ?? DateTime.UtcNow;

            var ingreso = new Ingreso
            {
                UserId = userId,
                CategoriaId = plantilla.CategoriaId,
                Descripcion = plantilla.Descripcion ?? plantilla.Nombre,
                Monto = montoFinal.Value,
                CuentaId = plantilla.CuentaId,
                Fecha = DateTime.SpecifyKind(fechaFinal, DateTimeKind.Utc)
            };
            _context.Ingresos.Add(ingreso);

            if (plantilla.CuentaId.HasValue && plantilla.Cuenta != null)
            {
                plantilla.Cuenta.BalanceActual += montoFinal.Value; // ADD for income
            }

            plantilla.VecesUsada++;
            await _context.SaveChangesAsync();

            var categoria = await _context.Categorias.FindAsync(plantilla.CategoriaId);

            return new
            {
                Id = ingreso.Id,
                Fecha = ingreso.Fecha,
                CategoriaId = ingreso.CategoriaId,
                CategoriaNombre = categoria?.Nombre ?? "",
                Descripcion = ingreso.Descripcion,
                Monto = ingreso.Monto,
                CuentaId = ingreso.CuentaId
            };
        }
    }
}
