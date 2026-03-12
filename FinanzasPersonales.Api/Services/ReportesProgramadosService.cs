using Microsoft.EntityFrameworkCore;
using FinanzasPersonales.Api.Data;
using FinanzasPersonales.Api.Dtos;
using FinanzasPersonales.Api.Models;
using System.Text.Json;

namespace FinanzasPersonales.Api.Services
{
    public interface IReportesProgramadosService
    {
        Task<List<ReporteProgramadoDto>> GetAllAsync(string userId);
        Task<ReporteProgramadoDto?> GetByIdAsync(string userId, int id);
        Task<ReporteProgramadoDto> CreateAsync(string userId, CreateReporteProgramadoDto dto);
        Task<ReporteProgramadoDto?> UpdateAsync(string userId, int id, UpdateReporteProgramadoDto dto);
        Task<bool> DeleteAsync(string userId, int id);
    }

    public class ReportesProgramadosService : IReportesProgramadosService
    {
        private readonly FinanzasDbContext _context;

        public ReportesProgramadosService(FinanzasDbContext context)
        {
            _context = context;
        }

        public async Task<List<ReporteProgramadoDto>> GetAllAsync(string userId)
        {
            return await _context.ReportesProgramados
                .Where(r => r.UserId == userId)
                .OrderByDescending(r => r.FechaCreacion)
                .Select(r => MapToDto(r))
                .ToListAsync();
        }

        public async Task<ReporteProgramadoDto?> GetByIdAsync(string userId, int id)
        {
            var reporte = await _context.ReportesProgramados
                .FirstOrDefaultAsync(r => r.Id == id && r.UserId == userId);

            return reporte == null ? null : MapToDto(reporte);
        }

        public async Task<ReporteProgramadoDto> CreateAsync(string userId, CreateReporteProgramadoDto dto)
        {
            var reporte = new ReporteProgramado
            {
                UserId = userId,
                Frecuencia = dto.Frecuencia,
                EmailDestino = dto.EmailDestino,
                SeccionesIncluir = JsonSerializer.Serialize(dto.SeccionesIncluir),
                Activo = true,
                FechaCreacion = DateTime.UtcNow
            };

            _context.ReportesProgramados.Add(reporte);
            await _context.SaveChangesAsync();

            return MapToDto(reporte);
        }

        public async Task<ReporteProgramadoDto?> UpdateAsync(string userId, int id, UpdateReporteProgramadoDto dto)
        {
            var reporte = await _context.ReportesProgramados
                .FirstOrDefaultAsync(r => r.Id == id && r.UserId == userId);

            if (reporte == null) return null;

            if (dto.Frecuencia != null) reporte.Frecuencia = dto.Frecuencia;
            if (dto.EmailDestino != null) reporte.EmailDestino = dto.EmailDestino;
            if (dto.SeccionesIncluir != null) reporte.SeccionesIncluir = JsonSerializer.Serialize(dto.SeccionesIncluir);
            if (dto.Activo.HasValue) reporte.Activo = dto.Activo.Value;

            await _context.SaveChangesAsync();

            return MapToDto(reporte);
        }

        public async Task<bool> DeleteAsync(string userId, int id)
        {
            var reporte = await _context.ReportesProgramados
                .FirstOrDefaultAsync(r => r.Id == id && r.UserId == userId);

            if (reporte == null) return false;

            _context.ReportesProgramados.Remove(reporte);
            await _context.SaveChangesAsync();
            return true;
        }

        private static ReporteProgramadoDto MapToDto(ReporteProgramado r)
        {
            return new ReporteProgramadoDto
            {
                Id = r.Id,
                Frecuencia = r.Frecuencia,
                EmailDestino = r.EmailDestino,
                SeccionesIncluir = JsonSerializer.Deserialize<List<string>>(r.SeccionesIncluir) ?? new(),
                Activo = r.Activo,
                UltimoEnvio = r.UltimoEnvio,
                FechaCreacion = r.FechaCreacion
            };
        }
    }
}
