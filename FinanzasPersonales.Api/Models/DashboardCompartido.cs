using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Identity;

namespace FinanzasPersonales.Api.Models
{
    public class DashboardCompartido
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string UserId { get; set; } = string.Empty;

        [ForeignKey("UserId")]
        public virtual IdentityUser? User { get; set; }

        [Required]
        [StringLength(64)]
        public string Token { get; set; } = string.Empty;

        [StringLength(100)]
        public string? NombreDestinatario { get; set; } // "Mi pareja", "Contador"

        public DateTime FechaCreacion { get; set; } = DateTime.UtcNow;

        public DateTime? FechaExpiracion { get; set; } // null = no expira

        public bool Activo { get; set; } = true;

        /// <summary>
        /// Secciones permitidas en JSON: ["dashboard","gastos","ingresos","presupuestos","metas","deudas"]
        /// </summary>
        [StringLength(500)]
        public string SeccionesPermitidas { get; set; } = "[\"dashboard\",\"gastos\",\"presupuestos\"]";
    }
}
