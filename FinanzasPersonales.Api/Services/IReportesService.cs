using FinanzasPersonales.Api.Dtos;

namespace FinanzasPersonales.Api.Services
{
    public interface IReportesService
    {
        Task<List<GastoPorCategoriaDto>> GetGastosPorCategoriaAsync(string userId, int? mes = null, int? ano = null);
        Task<List<EvolucionMensualDto>> GetEvolucionMensualAsync(string userId, int meses = 6);
        Task<ComparativaPeriodosDto> GetComparativaPeriodosAsync(string userId, int? mesActual = null, int? anoActual = null, int? mesAnterior = null, int? anoAnterior = null);
        Task<ResumenGeneralDto> GetResumenGeneralAsync(string userId, DateTime? desde = null, DateTime? hasta = null);
        Task<TendenciasMensualesDto> GetTendenciasAsync(string userId, int meses = 6);
        Task<ComparativaMesDto> GetComparativaAsync(string userId, int? mes = null, int? ano = null);
        Task<TopCategoriasDto> GetTopCategoriasAsync(string userId, int? mes = null, int? ano = null, int limite = 5);
        Task<GastosTipoDto> GetGastosTipoAsync(string userId, int? mes = null, int? ano = null);
        Task<ProyeccionGastosDto> GetProyeccionAsync(string userId);
        Task<CalendarioDto> GetCalendarioAsync(string userId, int mes, int ano);
        Task<ComparacionPeriodosDto> CompararPeriodosAsync(string userId, DateTime fecha1Inicio, DateTime fecha1Fin, DateTime fecha2Inicio, DateTime fecha2Fin);
    }
}
