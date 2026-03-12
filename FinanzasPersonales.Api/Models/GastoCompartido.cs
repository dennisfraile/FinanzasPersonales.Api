using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Identity;

namespace FinanzasPersonales.Api.Models
{
    public class GastoCompartido
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
        [Column(TypeName = "decimal(18, 2)")]
        public decimal MontoTotal { get; set; }

        [Required]
        public DateTime Fecha { get; set; }

        public int? CategoriaId { get; set; }

        [ForeignKey("CategoriaId")]
        public virtual Categoria? Categoria { get; set; }

        [StringLength(20)]
        public string MetodoDivision { get; set; } = "Equitativo"; // Equitativo, Porcentaje, MontoFijo

        public DateTime FechaCreacion { get; set; } = DateTime.UtcNow;

        public virtual ICollection<ParticipanteGasto> Participantes { get; set; } = new List<ParticipanteGasto>();
    }

    public class ParticipanteGasto
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int GastoCompartidoId { get; set; }

        [ForeignKey("GastoCompartidoId")]
        public virtual GastoCompartido? GastoCompartido { get; set; }

        [Required]
        [StringLength(100)]
        public string Nombre { get; set; } = string.Empty;

        [StringLength(200)]
        public string? Email { get; set; }

        [Required]
        [Column(TypeName = "decimal(18, 2)")]
        public decimal MontoAsignado { get; set; }

        [Column(TypeName = "decimal(18, 2)")]
        public decimal MontoPagado { get; set; } = 0;

        public bool Liquidado { get; set; } = false;

        public DateTime? FechaLiquidacion { get; set; }
    }
}
