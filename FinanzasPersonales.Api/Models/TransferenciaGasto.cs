using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FinanzasPersonales.Api.Models
{
    /// <summary>
    /// Registro histórico de transferencias de saldo entre gastos (redistribución de presupuesto).
    /// </summary>
    public class TransferenciaGasto
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string UserId { get; set; } = string.Empty;

        [Required]
        public int GastoOrigenId { get; set; }

        [ForeignKey("GastoOrigenId")]
        public virtual Gasto? GastoOrigen { get; set; }

        [Required]
        public int GastoDestinoId { get; set; }

        [ForeignKey("GastoDestinoId")]
        public virtual Gasto? GastoDestino { get; set; }

        [Required]
        [Column(TypeName = "decimal(18, 2)")]
        public decimal Monto { get; set; }

        /// <summary>
        /// CategoriaId del gasto origen (para consultas rápidas por presupuesto)
        /// </summary>
        public int CategoriaOrigenId { get; set; }

        /// <summary>
        /// CategoriaId del gasto destino
        /// </summary>
        public int CategoriaDestinoId { get; set; }

        public DateTime Fecha { get; set; } = DateTime.UtcNow;
    }
}
