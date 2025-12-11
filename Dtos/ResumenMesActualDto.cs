namespace FinanzasPersonales.Api.Dtos
{
    /// <summary>
    /// DTO para resumen del mes actual.
    /// </summary>
    public class ResumenMesActualDto
    {
        public int Mes { get; set; }
        public int Ano { get; set; }
        public decimal TotalIngresos { get; set; }
        public decimal TotalGastos { get; set; }
        public decimal Balance { get; set; }
        public decimal PromedioGastoDiario { get; set; }
        public int CantidadTransacciones { get; set; }
    }
}
