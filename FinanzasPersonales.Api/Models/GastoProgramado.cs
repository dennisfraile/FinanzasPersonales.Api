using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Identity;

namespace FinanzasPersonales.Api.Models
{
    /// <summary>
    /// Estados posibles de un gasto programado
    /// </summary>
    public enum EstadoGastoProgramado
    {
        Pendiente,
        Pagado,
        Vencido,
        Cancelado
    }

    /// <summary>
    /// Representa un gasto programado con fecha de vencimiento futura.
    /// Permite registrar recibos (agua, luz, gas) y otros cobros con fecha límite,
    /// donde el monto puede variar y el descuento en cuenta se aplica en la fecha de pago.
    /// </summary>
    public class GastoProgramado
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string UserId { get; set; } = string.Empty;

        [ForeignKey("UserId")]
        public virtual IdentityUser? User { get; set; }

        [Required]
        [StringLength(200)]
        public string Descripcion { get; set; } = string.Empty;

        [Required]
        public int CategoriaId { get; set; }

        [ForeignKey("CategoriaId")]
        public virtual Categoria? Categoria { get; set; }

        public int? CuentaId { get; set; }

        [ForeignKey("CuentaId")]
        public virtual Cuenta? Cuenta { get; set; }

        /// <summary>
        /// Monto estimado o programado del gasto
        /// </summary>
        [Required]
        [Column(TypeName = "decimal(18, 2)")]
        public decimal Monto { get; set; }

        /// <summary>
        /// Monto real pagado (puede diferir del monto programado en gastos variables)
        /// </summary>
        [Column(TypeName = "decimal(18, 2)")]
        public decimal? MontoPagado { get; set; }

        /// <summary>
        /// Indica si el monto puede variar (recibos de luz, agua, gas)
        /// vs monto fijo (suscripciones, renta)
        /// </summary>
        public bool EsMontoVariable { get; set; } = false;

        /// <summary>
        /// Fecha límite de pago del gasto
        /// </summary>
        [Required]
        public DateTime FechaVencimiento { get; set; }

        /// <summary>
        /// Fecha en que se realizó el pago efectivo
        /// </summary>
        public DateTime? FechaPago { get; set; }

        /// <summary>
        /// Estado actual del gasto programado
        /// </summary>
        [Required]
        [StringLength(20)]
        public string Estado { get; set; } = "Pendiente"; // Pendiente, Pagado, Vencido, Cancelado

        /// <summary>
        /// Referencia al GastoRecurrente que originó este gasto programado (si aplica)
        /// </summary>
        public int? GastoRecurrenteId { get; set; }

        [ForeignKey("GastoRecurrenteId")]
        public virtual GastoRecurrente? GastoRecurrente { get; set; }

        /// <summary>
        /// ID del Gasto generado cuando se efectúa el pago
        /// </summary>
        public int? GastoGeneradoId { get; set; }

        [StringLength(2000)]
        public string? Notas { get; set; }

        public DateTime FechaCreacion { get; set; } = DateTime.UtcNow;
    }
}
