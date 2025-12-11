namespace FinanzasPersonales.Api.Dtos
{
    /// <summary>
    /// DTO para comparar dos períodos financieros.
    /// </summary>
    public class ComparativaPeriodosDto
    {
        public required PeriodoFinanciero PeriodoActual { get; set; }
        public required PeriodoFinanciero PeriodoAnterior { get; set; }
        public decimal DiferenciaIngresos { get; set; }
        public decimal DiferenciaGastos { get; set; }
        public decimal DiferenciaBalance { get; set; }
        public decimal PorcentajeCambioIngresos { get; set; }
        public decimal PorcentajeCambioGastos { get; set; }
    }

    public class PeriodoFinanciero
    {
        public required string Descripcion { get; set; } // Ej: "Noviembre 2025"
        public decimal TotalIngresos { get; set; }
        public decimal TotalGastos { get; set; }
        public decimal Balance { get; set; }
    }
}
