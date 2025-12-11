using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations.Schema; // Para [ForeignKey]
using Microsoft.AspNetCore.Identity; // Para IdentityUser

namespace FinanzasPersonales.Api.Models
{
    public class Ingreso
    {
        [Key] // Indica que esta es la Clave Primaria (como el ID_Ingreso)
        public int Id { get; set; }

        [Required] // Anotación de validación: este campo no puede ser nulo
        public DateTime Fecha { get; set; }

        [Required]
        public int CategoriaId { get; set; } // La llave foránea

        [ForeignKey("CategoriaId")]
        public virtual Categoria Categoria { get; set; } // Propiedad de navegación

        [Required]
        [Column(TypeName = "decimal(18, 2)")] 
        public decimal Monto { get; set; }

        [Required]
        public string UserId { get; set; } // El ID del usuario de la tabla AspNetUsers

        [ForeignKey("UserId")]
        public virtual IdentityUser User { get; set; } // Propiedad de navegación
    }
}
