namespace FinanzasPersonales.Api.Dtos
{
    /// <summary>
    /// DTO para dashboard completo con todos los widgets.
    /// </summary>
    public class DashboardDto
    {
        public required ResumenMesActualDto MesActual { get; set; }
        public required List<EvolucionMensualDto> UltimosSeisMeses { get; set; }
        public required List<GastoPorCategoriaDto> TopCategorias { get; set; }
        public required List<PresupuestoDto> PresupuestosActivos { get; set; }
        public List<MetaDto>? MetasActivas { get; set; }
    }

    public class DashboardMetricsDto
    {
        public decimal TotalIngresosDelMes { get; set; }
        public decimal TotalGastosDelMes { get; set; }
        public decimal BalanceDelMes { get; set; }
        public decimal CambioMesAnterior { get; set; }

        public List<MesFinancieroDto> Tendencia6Meses { get; set; } = new();
        public List<CategoriaTopDto> Top5Categorias { get; set; } = new();
    }

    public class MesFinancieroDto
    {
        public string Mes { get; set; } = string.Empty;
        public decimal Ingresos { get; set; }
        public decimal Gastos { get; set; }
    }

    public class CategoriaTopDto
    {
        public string Nombre { get; set; } = string.Empty;
        public decimal Total { get; set; }
        public string Color { get; set; } = "#3b82f6";
    }
}
