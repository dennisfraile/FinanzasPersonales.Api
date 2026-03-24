using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FinanzasPersonales.Api.Models
{
    public class GastoCompartidoToken
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int GastoCompartidoId { get; set; }

        [ForeignKey("GastoCompartidoId")]
        public virtual GastoCompartido? GastoCompartido { get; set; }

        [Required]
        [StringLength(64)]
        public string Token { get; set; } = string.Empty;

        public DateTime FechaCreacion { get; set; } = DateTime.UtcNow;

        public DateTime FechaExpiracion { get; set; }

        public bool Activo { get; set; } = true;

        [Required]
        public string UserId { get; set; } = string.Empty;
    }
}
