using FinanzasPersonales.Api.Dtos;

namespace FinanzasPersonales.Api.Services
{
    public interface IPlantillasIngresoService
    {
        Task<List<PlantillaIngresoDto>> GetPlantillasAsync(string userId);
        Task<PlantillaIngresoDto> CreatePlantillaAsync(string userId, CreatePlantillaIngresoDto dto);
        Task<bool> UpdatePlantillaAsync(string userId, int id, UpdatePlantillaIngresoDto dto);
        Task<bool> DeletePlantillaAsync(string userId, int id);
        Task<object> UsarPlantillaAsync(string userId, int plantillaId, UsarPlantillaIngresoDto dto);
    }
}
