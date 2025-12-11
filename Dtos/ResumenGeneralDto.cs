namespace FinanzasPersonales.Api.Dtos
{
    /// <summary>
    /// DTO para resumen general de estadísticas financieras.
    /// </summary>
    public class ResumenGeneralDto
    {
        public required string PeriodoAnalizado { get; set; }
        public decimal TotalIngresos { get; set; }
        public decimal TotalGastos { get; set; }
        public decimal Balance { get; set; }
        public decimal PromedioIngresosDiario { get; set; }
        public decimal PromedioGastosDiario { get; set; }
        public decimal PromedioIngresosMensual { get; set; }
        public decimal PromedioGastosMensual { get; set; }
        public int DiasConActividad { get; set; }
        public required string CategoriaConMasGasto { get; set; }
        public decimal MontoCategoriaMasGasto { get; set; }
    }
}
