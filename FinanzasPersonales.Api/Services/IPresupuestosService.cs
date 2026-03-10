using FinanzasPersonales.Api.Dtos;
using FinanzasPersonales.Api.Models;

namespace FinanzasPersonales.Api.Services
{
    public interface IPresupuestosService
    {
        Task<List<PresupuestoDto>> GetPresupuestosAsync(string userId, int? mes = null, int? ano = null);
        Task<PresupuestoDto?> GetPresupuestoAsync(string userId, int id);
        Task<(PresupuestoDto? result, string? error)> CreatePresupuestoAsync(string userId, PresupuestoCreateDto dto);
        Task<bool> UpdatePresupuestoAsync(string userId, int id, PresupuestoUpdateDto dto);
        Task<bool> DeletePresupuestoAsync(string userId, int id);
        Task<List<PresupuestoDto>> GetAlertasAsync(string userId);
        Task<decimal> CalcularGastadoActual(string userId, Presupuesto presupuesto);
    }
}
