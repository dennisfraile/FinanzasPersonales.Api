using FinanzasPersonales.Api.Dtos;
using FinanzasPersonales.Api.Models;

namespace FinanzasPersonales.Api.Services
{
    public interface IGastosService
    {
        Task<PaginatedResponseDto<GastoDto>> GetGastosAsync(
            string userId,
            int? categoriaId = null,
            string? tipo = null,
            DateTime? desde = null,
            DateTime? hasta = null,
            decimal? montoMin = null,
            decimal? montoMax = null,
            string? descripcionContiene = null,
            string ordenarPor = "fecha",
            string ordenDireccion = "desc",
            int pagina = 1,
            int tamañoPagina = 50,
            List<int>? tagIds = null);

        Task<GastoDto?> GetGastoAsync(string userId, int id);

        Task<GastoDto> CreateGastoAsync(string userId, CreateGastoDto dto);

        Task<bool> UpdateGastoAsync(string userId, int id, UpdateGastoDto dto);

        Task<bool> DeleteGastoAsync(string userId, int id);
    }
}
