using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FinanzasPersonales.Api.Models
{
    /// <summary>
    /// Representa una compra individual dentro de un gasto asignado.
    /// Ejemplo: Un gasto de "Alimentación $45/semana" puede tener detalles como "Almuerzo $8", "Cena $12", etc.
    /// </summary>
    public class DetalleGasto
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int GastoId { get; set; }

        [ForeignKey("GastoId")]
        public virtual Gasto? Gasto { get; set; }

        [Required]
        public string UserId { get; set; } = string.Empty;

        [Required]
        [StringLength(250)]
        public string Descripcion { get; set; } = string.Empty;

        [Required]
        [Column(TypeName = "decimal(18, 2)")]
        public decimal Monto { get; set; }

        [Required]
        public DateTime Fecha { get; set; }

        [StringLength(500)]
        public string? Notas { get; set; }
    }
}
