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

        [StringLength(500)]
        public string? Descripcion { get; set; } // Descripción opcional del ingreso

        [Required]
        [Column(TypeName = "decimal(18, 2)")]
        public decimal Monto { get; set; }

        [Required]
        public string UserId { get; set; } // El ID del usuario de la tabla AspNetUsers

        [ForeignKey("UserId")]
        public virtual IdentityUser User { get; set; } // Propiedad de navegación

        // Relación con Cuenta
        public int? CuentaId { get; set; }

        [ForeignKey("CuentaId")]
        public virtual Cuenta? Cuenta { get; set; }

        [StringLength(2000)]
        public string? Notas { get; set; }

        // Multi-moneda
        [StringLength(3)]
        [RegularExpression(@"^[A-Z]{3}$", ErrorMessage = "Moneda debe ser un código ISO 4217 de 3 letras (ej: USD, EUR).")]
        public string? Moneda { get; set; }

        [Column(TypeName = "decimal(18, 6)")]
        public decimal? TipoCambioUsado { get; set; }

        [Column(TypeName = "decimal(18, 2)")]
        public decimal? MontoConvertido { get; set; }

        // Relación many-to-many con Tags
        public ICollection<IngresoTag> IngresoTags { get; set; } = new List<IngresoTag>();
    }
}
