using FinanzasPersonales.Api.Dtos;
using FinanzasPersonales.Api.Models;

namespace FinanzasPersonales.Api.Services
{
    public interface IIngresosRecurrentesService
    {
        Task<List<IngresoRecurrenteDto>> GetIngresosRecurrentesAsync(string userId);
        Task<IngresoRecurrenteDto?> GetIngresoRecurrenteAsync(string userId, int id);
        Task<(IngresoRecurrenteDto? result, string? error)> CreateIngresoRecurrenteAsync(string userId, CreateIngresoRecurrenteDto dto);
        Task<(bool success, string? error)> UpdateIngresoRecurrenteAsync(string userId, int id, UpdateIngresoRecurrenteDto dto);
        Task<bool> DeleteIngresoRecurrenteAsync(string userId, int id);
        Task<(Ingreso? result, string? error)> GenerarIngresoAsync(string userId, int id);
        Task<int> GenerarPendientesAsync(string userId);
    }
}
