namespace FinanzasPersonales.Api.Dtos
{
    public class DashboardResumenDto
    {
        // --- Resumen del Período Actual(Quincena) ---
        public string PeriodoActual { get; set; }
        public decimal IngresosAsignados { get; set; } // El dinero que estás usando (ej. de la Q2 anterior)
        public decimal AhorroBaseCalculado { get; set; } // El 10% de los ingresos asignados
        public decimal GastosFijosPagados { get; set; }
        public decimal GastosVariablesPagados { get; set; }
        public decimal SaldoDisponiblePeriodo { get; set; } // El dinero que te queda para ESTA quincena

        // --- Resumen Mensual (Para Metas) ---
        public decimal FlujoLibreMensual { get; set; } // El sobrante total del mes (reemplaza E10 de Excel)
    }
}
