using FinanzasPersonales.Api.Dtos;

namespace FinanzasPersonales.Api.Services
{
    public interface ICuentasService
    {
        Task<List<CuentaDto>> GetCuentasAsync(string userId);
        Task<CuentaDto?> GetCuentaAsync(string userId, int id);
        Task<CuentaDto> CreateCuentaAsync(string userId, CuentaCreateDto dto);
        Task<bool> UpdateCuentaAsync(string userId, int id, CuentaUpdateDto dto);
        Task<bool> DeleteCuentaAsync(string userId, int id);
        Task<decimal> GetBalanceTotalAsync(string userId);
    }
}
