using System.ComponentModel.DataAnnotations;

namespace FinanzasPersonales.Api.Dtos
{
    /// <summary>
    /// DTO para crear un gasto programado (recibo, cobro con fecha límite)
    /// </summary>
    public class CreateGastoProgramadoDto
    {
        [Required(ErrorMessage = "La descripción es requerida")]
        [StringLength(200)]
        public string Descripcion { get; set; } = string.Empty;

        [Required(ErrorMessage = "La categoría es requerida")]
        public int CategoriaId { get; set; }

        public int? CuentaId { get; set; }

        [Required(ErrorMessage = "El monto es requerido")]
        [Range(0.01, double.MaxValue, ErrorMessage = "El monto debe ser mayor a 0")]
        public decimal Monto { get; set; }

        /// <summary>
        /// Indica si el monto puede variar (recibos de servicios) o es fijo (suscripciones)
        /// </summary>
        public bool EsMontoVariable { get; set; } = false;

        [Required(ErrorMessage = "La fecha de vencimiento es requerida")]
        public DateTime FechaVencimiento { get; set; }

        [StringLength(2000)]
        public string? Notas { get; set; }
    }

    /// <summary>
    /// DTO para actualizar un gasto programado (ej: actualizar monto de recibo variable)
    /// </summary>
    public class UpdateGastoProgramadoDto
    {
        [Required(ErrorMessage = "La descripción es requerida")]
        [StringLength(200)]
        public string Descripcion { get; set; } = string.Empty;

        [Required(ErrorMessage = "La categoría es requerida")]
        public int CategoriaId { get; set; }

        public int? CuentaId { get; set; }

        [Required(ErrorMessage = "El monto es requerido")]
        [Range(0.01, double.MaxValue, ErrorMessage = "El monto debe ser mayor a 0")]
        public decimal Monto { get; set; }

        public bool EsMontoVariable { get; set; }

        [Required(ErrorMessage = "La fecha de vencimiento es requerida")]
        public DateTime FechaVencimiento { get; set; }

        [StringLength(2000)]
        public string? Notas { get; set; }
    }

    /// <summary>
    /// DTO para registrar el pago de un gasto programado
    /// </summary>
    public class PagarGastoProgramadoDto
    {
        /// <summary>
        /// Monto real pagado. Si no se especifica, se usa el monto programado.
        /// </summary>
        [Range(0.01, double.MaxValue, ErrorMessage = "El monto debe ser mayor a 0")]
        public decimal? MontoPagado { get; set; }

        /// <summary>
        /// Cuenta desde la que se paga. Si no se especifica, se usa la cuenta asignada.
        /// </summary>
        public int? CuentaId { get; set; }

        /// <summary>
        /// Fecha del pago. Si no se especifica, se usa la fecha actual.
        /// </summary>
        public DateTime? FechaPago { get; set; }
    }

    /// <summary>
    /// DTO de respuesta con la información completa del gasto programado
    /// </summary>
    public class GastoProgramadoDto
    {
        public int Id { get; set; }
        public string Descripcion { get; set; } = string.Empty;
        public int CategoriaId { get; set; }
        public string? CategoriaNombre { get; set; }
        public int? CuentaId { get; set; }
        public string? CuentaNombre { get; set; }
        public decimal Monto { get; set; }
        public decimal? MontoPagado { get; set; }
        public bool EsMontoVariable { get; set; }
        public DateTime FechaVencimiento { get; set; }
        public DateTime? FechaPago { get; set; }
        public string Estado { get; set; } = string.Empty;
        public int? GastoRecurrenteId { get; set; }
        public int? GastoGeneradoId { get; set; }
        public string? Notas { get; set; }
        public DateTime FechaCreacion { get; set; }

        /// <summary>
        /// Días restantes hasta el vencimiento (negativo si ya venció)
        /// </summary>
        public int DiasParaVencimiento { get; set; }
    }
}
