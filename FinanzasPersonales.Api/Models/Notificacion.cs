using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Identity;

namespace FinanzasPersonales.Api.Models
{
    /// <summary>
    /// Representa una notificación para el usuario.
    /// </summary>
    public class Notificacion
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public required string UserId { get; set; }

        [ForeignKey("UserId")]
        public virtual IdentityUser? User { get; set; }

        /// <summary>
        /// Tipo de notificación: "PresupuestoAlerta", "MetaProxima", "MetaCumplida"
        /// </summary>
        [Required]
        [StringLength(50)]
        public required string Tipo { get; set; }

        [Required]
        [StringLength(200)]
        public required string Titulo { get; set; }

        [Required]
        [StringLength(1000)]
        public required string Mensaje { get; set; }

        [Required]
        public DateTime FechaCreacion { get; set; }

        public bool Leida { get; set; }

        public bool EmailEnviado { get; set; }

        public DateTime? FechaEmailEnviado { get; set; }
    }
}
