using FinanzasPersonales.Api.Dtos;

namespace FinanzasPersonales.Api.Services
{
    public interface IGastosProgramadosService
    {
        Task<List<GastoProgramadoDto>> GetGastosProgramadosAsync(string userId, string? estado = null);
        Task<GastoProgramadoDto?> GetGastoProgramadoAsync(string userId, int id);
        Task<(GastoProgramadoDto? result, string? error)> CreateGastoProgramadoAsync(string userId, CreateGastoProgramadoDto dto);
        Task<(bool success, string? error)> UpdateGastoProgramadoAsync(string userId, int id, UpdateGastoProgramadoDto dto);
        Task<bool> DeleteGastoProgramadoAsync(string userId, int id);
        Task<(GastoProgramadoDto? result, string? error)> PagarGastoProgramadoAsync(string userId, int id, PagarGastoProgramadoDto dto);
        Task<(bool success, string? error)> CancelarGastoProgramadoAsync(string userId, int id);
        Task<int> MarcarVencidosAsync();
        Task<int> ProcesarCobrosAutomaticosAsync();
        Task<decimal> GetTotalComprometidoAsync(string userId, int categoriaId, DateTime inicio, DateTime fin);
    }
}
