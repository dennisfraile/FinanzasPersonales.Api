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

        /// <summary>
        /// Monto comprometido en gastos programados pendientes dentro del periodo
        /// </summary>
        public decimal Comprometido { get; set; }

        /// <summary>
        /// Total proyectado = GastadoActual + Comprometido
        /// </summary>
        public decimal TotalProyectado { get; set; }

        /// <summary>
        /// Porcentaje proyectado incluyendo comprometidos
        /// </summary>
        public decimal PorcentajeProyectado { get; set; }

        /// <summary>
        /// Transferencias de saldo que afectan esta categoría en el periodo
        /// </summary>
        public List<TransferenciaGastoResumenDto> Transferencias { get; set; } = new();

        /// <summary>
        /// Si el presupuesto permite acumular sobrante del periodo anterior
        /// </summary>
        public bool PermiteRollover { get; set; }

        /// <summary>
        /// Monto acumulado del periodo anterior (sobrante que se sumó al límite)
        /// </summary>
        public decimal Rollover { get; set; }

        /// <summary>
        /// Límite efectivo = MontoLimite + Rollover
        /// </summary>
        public decimal LimiteEfectivo { get; set; }
    }

    /// <summary>
    /// Resumen de una transferencia de saldo entre gastos de diferentes categorías
    /// </summary>
    public class TransferenciaGastoResumenDto
    {
        public int Id { get; set; }
        public decimal Monto { get; set; }
        /// <summary>
        /// Nombre de la categoría origen
        /// </summary>
        public string CategoriaOrigenNombre { get; set; } = string.Empty;
        /// <summary>
        /// Nombre de la categoría destino
        /// </summary>
        public string CategoriaDestinoNombre { get; set; } = string.Empty;
        /// <summary>
        /// "entrada" si esta categoría recibió, "salida" si esta categoría cedió
        /// </summary>
        public string Direccion { get; set; } = string.Empty;
        public DateTime Fecha { get; set; }
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
