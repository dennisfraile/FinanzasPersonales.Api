namespace FinanzasPersonales.Api.Dtos
{
    public class FlujoCajaDto
    {
        public decimal BalanceActualTotal { get; set; }
        public List<ProyeccionFlujoCajaDto> Proyecciones { get; set; } = new();
    }

    public class ProyeccionFlujoCajaDto
    {
        public string Periodo { get; set; } = string.Empty;
        public int Dias { get; set; }
        public DateTime FechaHasta { get; set; }
        public decimal IngresosEsperados { get; set; }
        public decimal GastosEsperados { get; set; }
        public decimal PagosDeudaEsperados { get; set; }
        public decimal BalanceProyectado { get; set; }
    }
}
