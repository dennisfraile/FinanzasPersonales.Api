using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Identity;

namespace FinanzasPersonales.Api.Models
{
    public class Gasto
    {
        [Key] // Indica que esta es la Clave Primaria (como el ID_Gasto)
        public int Id { get; set; }

        [Required] // Anotación de validación: este campo no puede ser nulo
        public DateTime Fecha { get; set; }

        [Required]
        public int CategoriaId { get; set; } // La llave foránea

        [ForeignKey("CategoriaId")]
        public virtual Categoria Categoria { get; set; } = null!;

        [StringLength(50)]
        public string? Tipo { get; set; } = "Variable"; // "Fijo" o "Variable", default Variable

        [StringLength(250)]
        public string? Descripcion { get; set; } // '?' permite valores nulos

        [Required]
        [Column(TypeName = "decimal(18, 2)")]
        public decimal Monto { get; set; }

        [Required]
        public string UserId { get; set; } = null!;

        [ForeignKey("UserId")]
        public virtual IdentityUser User { get; set; } = null!;

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
        public ICollection<GastoTag> GastoTags { get; set; } = new List<GastoTag>();

        // Detalles de compras individuales dentro de este gasto
        public virtual ICollection<DetalleGasto> Detalles { get; set; } = new List<DetalleGasto>();
    }
}
