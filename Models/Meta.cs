using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
namespace FinanzasPersonales.Api.Models
{
    public class Meta
    {

        [Key] // Indica que esta es la Clave Primaria (como el ID_Meta)
        public int Id { get; set; }

        [Required]
        [StringLength(100)] // Límite de 100 caracteres
        public string Metas { get; set; }

        [Required]
        [Column(TypeName = "decimal(18, 2)")]
        public decimal MontoTotal { get; set; }

        [Required]
        [Column(TypeName = "decimal(18, 2)")]
        public decimal AhorroActual { get; set; }

        [Required]
        [Column(TypeName = "decimal(18, 2)")]
        public decimal MontoRestante { get; set; }
    }
}
