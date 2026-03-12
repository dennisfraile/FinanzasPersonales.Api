using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FinanzasPersonales.Api.Models
{
    public class TipoCambio
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(10)]
        public string MonedaOrigen { get; set; } = string.Empty;

        [Required]
        [StringLength(10)]
        public string MonedaDestino { get; set; } = string.Empty;

        [Required]
        [Column(TypeName = "decimal(18, 6)")]
        public decimal Tasa { get; set; }

        [Required]
        public DateTime Fecha { get; set; } = DateTime.UtcNow;

        [StringLength(50)]
        public string Fuente { get; set; } = "Manual";
    }
}
