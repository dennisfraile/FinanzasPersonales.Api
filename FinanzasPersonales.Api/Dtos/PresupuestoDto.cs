namespace FinanzasPersonales.Api.Dtos
{
    /// <summary>
    /// DTO para mostrar información de un presupuesto con cálculos.
    /// </summary>
    public class PresupuestoDto
    {
        public int Id { get; set; }
        public int CategoriaId { get; set; }
        public required string CategoriaNombre { get; set; }
        public decimal MontoLimite { get; set; }
        public required string Periodo { get; set; }
        public int MesAplicable { get; set; }
        public int AnoAplicable { get; set; }
        public int? SemanaAplicable { get; set; }

        // Calculados
        public decimal GastadoActual { get; set; }
        public decimal Disponible { get; set; }
        public decimal PorcentajeUtilizado { get; set; }
        public DateTime FechaInicio { get; set; }
        public DateTime FechaFin { get; set; }
    }

    /// <summary>
    /// DTO para el dashboard de presupuestos con comparación visual.
    /// </summary>
    public class PresupuestoDashboardDto
    {
        public required string Periodo { get; set; }
        public required string PeriodoLabel { get; set; }
        public DateTime FechaInicio { get; set; }
        public DateTime FechaFin { get; set; }
        public decimal TotalPresupuestado { get; set; }
        public decimal TotalGastado { get; set; }
        public decimal TotalDisponible { get; set; }
        public List<PresupuestoComparacionDto> Comparaciones { get; set; } = new();
    }

    public class PresupuestoComparacionDto
    {
        public int PresupuestoId { get; set; }
        public int CategoriaId { get; set; }
        public required string CategoriaNombre { get; set; }
        public decimal MontoLimite { get; set; }
        public decimal GastadoActual { get; set; }
        public decimal Disponible { get; set; }
        public decimal PorcentajeUtilizado { get; set; }
    }
}
