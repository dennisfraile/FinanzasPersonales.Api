using FinanzasPersonales.Api.Dtos;
using FinanzasPersonales.Api.Models;

namespace FinanzasPersonales.Api.Services
{
    public interface IGastosRecurrentesService
    {
        Task<List<GastoRecurrenteDto>> GetGastosRecurrentesAsync(string userId);
        Task<GastoRecurrenteDto?> GetGastoRecurrenteAsync(string userId, int id);
        Task<(GastoRecurrenteDto? result, string? error)> CreateGastoRecurrenteAsync(string userId, CreateGastoRecurrenteDto dto);
        Task<(bool success, string? error)> UpdateGastoRecurrenteAsync(string userId, int id, UpdateGastoRecurrenteDto dto);
        Task<bool> DeleteGastoRecurrenteAsync(string userId, int id);
        Task<(Gasto? result, string? error)> GenerarGastoAsync(string userId, int id);
        Task<int> GenerarPendientesAsync(string userId);
    }
}
