using FinanzasPersonales.Api.Dtos;

namespace FinanzasPersonales.Api.Services
{
    public interface ITransferenciasService
    {
        Task<List<TransferenciaDto>> GetTransferenciasAsync(string userId);
        Task<(TransferenciaDto? result, string? error)> CreateTransferenciaAsync(string userId, TransferenciaCreateDto dto);
    }
}
