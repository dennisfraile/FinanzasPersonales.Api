using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Identity;

namespace FinanzasPersonales.Api.Models
{
    public class ReporteProgramado
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string UserId { get; set; } = string.Empty;

        [ForeignKey("UserId")]
        public virtual IdentityUser? User { get; set; }

        [Required]
        [StringLength(20)]
        public string Frecuencia { get; set; } = "Mensual"; // Semanal, Mensual

        [Required]
        [StringLength(250)]
        public string EmailDestino { get; set; } = string.Empty;

        /// <summary>
        /// JSON array con las secciones a incluir: ["gastos","ingresos","presupuestos","metas"]
        /// </summary>
        [Required]
        [StringLength(500)]
        public string SeccionesIncluir { get; set; } = "[\"gastos\",\"ingresos\"]";

        public bool Activo { get; set; } = true;

        public DateTime? UltimoEnvio { get; set; }

        public DateTime FechaCreacion { get; set; } = DateTime.UtcNow;
    }
}
