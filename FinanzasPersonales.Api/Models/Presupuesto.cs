using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Identity;

namespace FinanzasPersonales.Api.Models
{
    /// <summary>
    /// Representa un presupuesto establecido para una categoría específica.
    /// </summary>
    public class Presupuesto
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int CategoriaId { get; set; }

        [ForeignKey("CategoriaId")]
        public virtual Categoria? Categoria { get; set; }

        [Required]
        [Column(TypeName = "decimal(18, 2)")]
        public decimal MontoLimite { get; set; }

        [Required]
        [StringLength(50)]
        public required string Periodo { get; set; } // "Semanal", "Quincenal", "Mensual", "Trimestral", "Semestral", "Anual"

        [Required]
        [Range(1, 12)]
        public int MesAplicable { get; set; } // 1-12

        [Required]
        public int AnoAplicable { get; set; } // Ej: 2025

        public int? SemanaAplicable { get; set; } // 1-53, solo para periodo Semanal

        [Required]
        public required string UserId { get; set; }

        [ForeignKey("UserId")]
        public virtual IdentityUser? User { get; set; }
    }
}
