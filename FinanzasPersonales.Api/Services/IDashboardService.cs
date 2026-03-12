using FinanzasPersonales.Api.Dtos;

namespace FinanzasPersonales.Api.Services
{
    public interface IDashboardService
    {
        Task<DashboardDto> GetDashboardAsync(string userId, int? mes = null, int? ano = null);
        Task<GraficaDto> GetGraficaIngresosVsGastosAsync(string userId, int meses = 6);
        Task<GraficaDto> GetGraficaGastosPorCategoriaAsync(string userId, int? mes = null, int? ano = null);
        Task<GraficaDto> GetGraficaProgresoMetasAsync(string userId);
        Task<DashboardMetricsDto> GetMetricsAsync(string userId);
        Task<FlujoCajaDto> GetFlujoCajaAsync(string userId);
    }
}
