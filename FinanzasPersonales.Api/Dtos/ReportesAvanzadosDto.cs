namespace FinanzasPersonales.Api.Dtos
{
    /// <summary>
    /// DTO para datos de tendencias mensuales
    /// </summary>
    public class TendenciasMensualesDto
    {
        public PeriodoDto Periodo { get; set; } = new();
        public List<DatoMensualDto> Datos { get; set; } = new();
    }

    /// <summary>
    /// DTO para periodo de reporte
    /// </summary>
    public class PeriodoDto
    {
        public DateTime Inicio { get; set; }
        public DateTime Fin { get; set; }
    }

    /// <summary>
    /// DTO para datos de un mes específico
    /// </summary>
    public class DatoMensualDto
    {
        public int Mes { get; set; }
        public int Ano { get; set; }
        public string Periodo { get; set; } = string.Empty;
        public decimal TotalIngresos { get; set; }
        public decimal TotalGastos { get; set; }
        public decimal Balance { get; set; }
    }

    /// <summary>
    /// DTO para comparativa entre dos meses
    /// </summary>
    public class ComparativaMesDto
    {
        public ResumenMesDto MesActual { get; set; } = new();
        public ResumenMesDto MesAnterior { get; set; } = new();
        public CambiosDto Cambios { get; set; } = new();
        public List<ComparativaCategoriaDto> Categorias { get; set; } = new();
    }

    /// <summary>
    /// DTO para resumen de un mes
    /// </summary>
    public class ResumenMesDto
    {
        public int Mes { get; set; }
        public int Ano { get; set; }
        public decimal TotalIngresos { get; set; }
        public decimal TotalGastos { get; set; }
        public decimal Balance { get; set; }
    }

    /// <summary>
    /// DTO para cambios porcentuales
    /// </summary>
    public class CambiosDto
    {
        public decimal IngresosPorcentaje { get; set; }
        public decimal GastosPorcentaje { get; set; }
        public decimal BalancePorcentaje { get; set; }
    }

    /// <summary>
    /// DTO para comparativa por categoría
    /// </summary>
    public class ComparativaCategoriaDto
    {
        public int CategoriaId { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public decimal MesActual { get; set; }
        public decimal MesAnterior { get; set; }
        public decimal Cambio { get; set; }
    }

    /// <summary>
    /// DTO para top categorías
    /// </summary>
    public class TopCategoriasDto
    {
        public int Mes { get; set; }
        public int Ano { get; set; }
        public decimal TotalGastos { get; set; }
        public List<CategoriaGastoDto> Categorias { get; set; } = new();
    }

    /// <summary>
    /// DTO para categoría con estadísticas
    /// </summary>
    public class CategoriaGastoDto
    {
        public int CategoriaId { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public decimal Total { get; set; }
        public decimal Porcentaje { get; set; }
        public int CantidadTransacciones { get; set; }
    }

    /// <summary>
    /// DTO para análisis de gastos por tipo
    /// </summary>
    public class GastosTipoDto
    {
        public int Mes { get; set; }
        public int Ano { get; set; }
        public GastosPorTipoDetalleDto GastosFijos { get; set; } = new();
        public GastosPorTipoDetalleDto GastosVariables { get; set; } = new();
        public decimal TotalGastos { get; set; }
    }

    /// <summary>
    /// DTO para detalle de gastos por tipo
    /// </summary>
    public class GastosPorTipoDetalleDto
    {
        public decimal Total { get; set; }
        public decimal Porcentaje { get; set; }
        public decimal Promedio { get; set; }
    }

    /// <summary>
    /// DTO para proyección de gastos
    /// </summary>
    public class ProyeccionGastosDto
    {
        public MesActualDto MesActual { get; set; } = new();
        public ProyeccionDetalleDto Proyeccion { get; set; } = new();
    }

    /// <summary>
    /// DTO para datos del mes actual
    /// </summary>
    public class MesActualDto
    {
        public int Mes { get; set; }
        public int Ano { get; set; }
        public decimal GastosActuales { get; set; }
        public int DiasTranscurridos { get; set; }
        public int DiasTotales { get; set; }
    }

    /// <summary>
    /// DTO para detalle de proyección
    /// </summary>
    public class ProyeccionDetalleDto
    {
        public decimal GastoEstimado { get; set; }
        public decimal PromedioUltimos3Meses { get; set; }
        public decimal Diferencia { get; set; }
        public decimal PorcentajeIncremento { get; set; }
        public bool Alerta { get; set; }
        public string Mensaje { get; set; } = string.Empty;
    }
}
