namespace FinanzasPersonales.Api.Dtos
{
    public class TransaccionTimelineDto
    {
        public int Id { get; set; }
        public DateTime Fecha { get; set; }
        public string Tipo { get; set; } = string.Empty; // "Ingreso" | "Gasto" | "TransferenciaEntrada" | "TransferenciaSalida"
        public string Descripcion { get; set; } = string.Empty;
        public string? Categoria { get; set; }
        public decimal Monto { get; set; }
        public decimal BalanceDespues { get; set; }
        public bool EsRecurrente { get; set; }
    }

    public class RecurrenteProximoDto
    {
        public int Id { get; set; }
        public string Tipo { get; set; } = string.Empty; // "Ingreso" | "Gasto"
        public string Descripcion { get; set; } = string.Empty;
        public decimal Monto { get; set; }
        public DateTime ProximaFecha { get; set; }
        public string Frecuencia { get; set; } = string.Empty;
    }

    public class ResumenMensualCuentaDto
    {
        public int Mes { get; set; }
        public int Ano { get; set; }
        public string Periodo { get; set; } = string.Empty;
        public decimal TotalIngresos { get; set; }
        public decimal TotalGastos { get; set; }
        public decimal Balance { get; set; }
    }

    public class SurplusQuincenaDto
    {
        public string Periodo { get; set; } = string.Empty;
        public DateTime FechaInicio { get; set; }
        public DateTime FechaFin { get; set; }
        public decimal TotalIngresos { get; set; }
        public decimal TotalGastos { get; set; }
        public decimal Surplus { get; set; }
        public bool PeriodoTerminado { get; set; }
    }

    public class CuentaDashboardDto
    {
        public int CuentaId { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public string Tipo { get; set; } = string.Empty;
        public decimal BalanceActual { get; set; }
        public decimal BalanceInicial { get; set; }
        public string Moneda { get; set; } = "USD";
        public string? Color { get; set; }

        public List<TransaccionTimelineDto> Transacciones { get; set; } = new();
        public int TotalTransacciones { get; set; }

        public List<RecurrenteProximoDto> Proximos { get; set; } = new();
        public List<ResumenMensualCuentaDto> ResumenMensual { get; set; } = new();
        public SurplusQuincenaDto? SurplusActual { get; set; }
    }

    public class AsignarSurplusDto
    {
        public int CuentaId { get; set; }
        public string Destino { get; set; } = string.Empty; // "BalanceInicial" | "Meta"
        public int? MetaId { get; set; }
        public decimal Monto { get; set; }
    }
}
