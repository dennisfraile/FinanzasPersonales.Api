using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Identity;

namespace FinanzasPersonales.Api.Models
{
    public class PlantillaGasto
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string UserId { get; set; } = string.Empty;

        [ForeignKey("UserId")]
        public virtual IdentityUser? User { get; set; }

        [Required]
        [StringLength(100)]
        public string Nombre { get; set; } = string.Empty;

        [Required]
        public int CategoriaId { get; set; }

        [ForeignKey("CategoriaId")]
        public virtual Categoria? Categoria { get; set; }

        [Column(TypeName = "decimal(18, 2)")]
        public decimal? Monto { get; set; }

        [StringLength(250)]
        public string? Descripcion { get; set; }

        [StringLength(50)]
        public string? Tipo { get; set; } = "Variable";

        public int? CuentaId { get; set; }

        [StringLength(50)]
        public string? Icono { get; set; }

        [StringLength(20)]
        public string? Color { get; set; }

        public int OrdenDisplay { get; set; } = 0;

        public int VecesUsada { get; set; } = 0;

        public DateTime FechaCreacion { get; set; } = DateTime.UtcNow;
    }
}
