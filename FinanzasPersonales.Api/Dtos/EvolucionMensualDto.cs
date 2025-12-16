namespace FinanzasPersonales.Api.Dtos
{
    /// <summary>
    /// DTO para reportes de evolución mensual de ingresos y gastos.
    /// </summary>
    public class EvolucionMensualDto
    {
        public int Mes { get; set; }
        public int Ano { get; set; }
        public required string Periodo { get; set; } // Ej: "Noviembre 2025"
        public decimal TotalIngresos { get; set; }
        public decimal TotalGastos { get; set; }
        public decimal Balance { get; set; }
        public decimal AhorroCalculado { get; set; }
    }
}
