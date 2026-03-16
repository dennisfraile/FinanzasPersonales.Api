using FinanzasPersonales.Api.Dtos;

namespace FinanzasPersonales.Api.Services
{
    /// <summary>
    /// Servicio para gestionar los detalles (sub-compras) de un gasto.
    /// </summary>
    public interface IDetallesGastoService
    {
        Task<List<DetalleGastoDto>> GetDetallesAsync(string userId, int gastoId);
        Task<GastoConDetallesDto?> GetGastoConDetallesAsync(string userId, int gastoId);
        Task<DetalleGastoDto> CreateDetalleAsync(string userId, int gastoId, CreateDetalleGastoDto dto);
        Task<bool> UpdateDetalleAsync(string userId, int gastoId, int detalleId, CreateDetalleGastoDto dto);
        Task<bool> DeleteDetalleAsync(string userId, int gastoId, int detalleId);
    }
}
