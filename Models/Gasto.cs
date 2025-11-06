using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
namespace FinanzasPersonales.Api.Models
{
    public class Gasto
    {
        [Key] // Indica que esta es la Clave Primaria (como el ID_Gasto)
        public int Id { get; set; }

        [Required] // Anotación de validación: este campo no puede ser nulo
        public DateTime Fecha { get; set; }

        [Required]
        [StringLength(100)] // Límite de 100 caracteres
        public string Categoria { get; set; }

        [Required]
        [StringLength(50)]
        public string Tipo { get; set; } // "Fijo" o "Variable"

        [StringLength(250)]
        public string? Descripcion { get; set; } // '?' permite valores nulos

        [Required]
        [Column(TypeName = "decimal(18, 2)")] 
        public decimal Monto { get; set; }
    }
}
