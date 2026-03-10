namespace FinanzasPersonales.Api.Dtos
{
    public class DiaCalendarioDto
    {
        public DateTime Fecha { get; set; }
        public decimal TotalIngresos { get; set; }
        public decimal TotalGastos { get; set; }
        public decimal Balance { get; set; }
        public int CantidadTransacciones { get; set; }
        public List<TransaccionSummaryDto> Transacciones { get; set; } = new();
    }

    public class TransaccionSummaryDto
    {
        public int Id { get; set; }
        public string Tipo { get; set; } = string.Empty; // "Gasto" | "Ingreso"
        public string Descripcion { get; set; } = string.Empty;
        public decimal Monto { get; set; }
        public string? CategoriaNombre { get; set; }
    }

    public class CalendarioDto
    {
        public int Mes { get; set; }
        public int Ano { get; set; }
        public List<DiaCalendarioDto> Dias { get; set; } = new();
    }
}
