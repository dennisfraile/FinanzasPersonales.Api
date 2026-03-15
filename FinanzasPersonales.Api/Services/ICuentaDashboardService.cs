using FinanzasPersonales.Api.Dtos;

namespace FinanzasPersonales.Api.Services
{
    public interface ICuentaDashboardService
    {
        Task<CuentaDashboardDto?> GetCuentaDashboardAsync(string userId, int cuentaId, int page = 1, int pageSize = 50);
        Task<(bool success, string? error)> AsignarSurplusAsync(string userId, AsignarSurplusDto dto);
    }
}
