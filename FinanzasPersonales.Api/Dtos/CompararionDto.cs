namespace FinanzasPersonales.Api.Dtos
{
    public class ComparacionPeriodosDto
    {
        public PeriodoFinancieroDto Periodo1 { get; set; } = new();
        public PeriodoFinancieroDto Periodo2 { get; set; } = new();
        public decimal DiferenciaIngresos { get; set; }
        public decimal DiferenciaGastos { get; set; }
        public decimal DiferenciaBalance { get; set; }
        public decimal PorcentajeCambioIngresos { get; set; }
        public decimal PorcentajeCambioGastos { get; set; }
    }

    public class PeriodoFinancieroDto
    {
        public DateTime FechaInicio { get; set; }
        public DateTime FechaFin { get; set; }
        public decimal TotalIngresos { get; set; }
        public decimal TotalGastos { get; set; }
        public decimal Balance { get; set; }
        public int CantidadIngresos { get; set; }
        public int CantidadGastos { get; set; }
    }
}
