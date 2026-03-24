using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Identity;

namespace FinanzasPersonales.Api.Models
{
    public class Meta
    {

        [Key] // Indica que esta es la Clave Primaria (como el ID_Meta)
        public int Id { get; set; }

        [Required]
        [StringLength(100)] // Límite de 100 caracteres
        public string Metas { get; set; } = null!;

        [Required]
        [Column(TypeName = "decimal(18, 2)")]
        public decimal MontoTotal { get; set; }

        [Required]
        [Column(TypeName = "decimal(18, 2)")]
        public decimal AhorroActual { get; set; }

        [Required]
        [Column(TypeName = "decimal(18, 2)")]
        public decimal MontoRestante { get; set; }

        [Required]
        public string UserId { get; set; } = null!;

        [ForeignKey("UserId")]
        public virtual IdentityUser User { get; set; } = null!;

        // Relación con Cuenta (opcional: en qué cuenta está el ahorro)
        public int? CuentaId { get; set; }

        [ForeignKey("CuentaId")]
        public virtual Cuenta? Cuenta { get; set; }

        // Auto-contribución recurrente
        public bool AbonoAutomatico { get; set; } = false;

        [Column(TypeName = "decimal(18, 2)")]
        public decimal? MontoAbono { get; set; }

        [StringLength(50)]
        public string? FrecuenciaAbono { get; set; } // "Semanal", "Quincenal", "Mensual"

        public int? DiaAbono { get; set; } // Día del mes o semana para el abono

        public DateTime? ProximoAbono { get; set; }

        public DateTime? UltimoAbono { get; set; }
    }
}
