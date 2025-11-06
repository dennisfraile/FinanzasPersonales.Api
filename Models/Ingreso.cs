using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
namespace FinanzasPersonales.Api.Models
{
    public class Ingreso
    {
        [Key] // Indica que esta es la Clave Primaria (como el ID_Ingreso)
        public int Id { get; set; }

        [Required] // Anotación de validación: este campo no puede ser nulo
        public DateTime Fecha { get; set; }

        [Required]
        [StringLength(100)] // Límite de 100 caracteres
        public string Fuente { get; set; }

        [Required]
        [Column(TypeName = "decimal(18, 2)")] 
        public decimal Monto { get; set; }
    }
}
