using FinanzasPersonales.Api.Dtos;
using FinanzasPersonales.Api.Models;

namespace FinanzasPersonales.Api.Services
{
    public interface IIngresosService
    {
        Task<PaginatedResponseDto<IngresoDto>> GetIngresosAsync(
            string userId,
            int? categoriaId = null,
            DateTime? desde = null,
            DateTime? hasta = null,
            decimal? montoMin = null,
            decimal? montoMax = null,
            string ordenarPor = "fecha",
            string ordenDireccion = "desc",
            int pagina = 1,
            int tamañoPagina = 50,
            List<int>? tagIds = null);

        Task<Ingreso?> GetIngresoAsync(string userId, int id);

        Task<Ingreso> CreateIngresoAsync(string userId, CreateIngresoDto dto);

        Task<bool> UpdateIngresoAsync(string userId, int id, UpdateIngresoDto dto);

        Task<bool> DeleteIngresoAsync(string userId, int id);
    }
}
