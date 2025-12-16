using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Identity;

namespace FinanzasPersonales.Api.Models
{
    /// <summary>
    /// Configuración personalizada del usuario.
    /// </summary>
    public class ConfiguracionUsuario
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public required string UserId { get; set; }

        [ForeignKey("UserId")]
        public virtual IdentityUser? User { get; set; }

        [StringLength(10)]
        public string Moneda { get; set; } = "USD";

        [StringLength(5)]
        public string SimboloMoneda { get; set; } = "$";

        [StringLength(5)]
        public string Idioma { get; set; } = "es";

        [StringLength(20)]
        public string Tema { get; set; } = "light";

        [Range(1, 31)]
        public int DiaInicioMes { get; set; } = 1;

        public bool MostrarSaldoInicial { get; set; } = true;

        [Required]
        public DateTime FechaCreacion { get; set; }

        public DateTime? FechaActualizacion { get; set; }
    }
}
