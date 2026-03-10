using System.ComponentModel.DataAnnotations;

namespace FinanzasPersonales.Api.Dtos
{
    public class GastoCreateDto
    {
        [Required]
        public DateTime Fecha { get; set; }

        [Required]
        public int CategoriaId { get; set; } // <-- El gran cambio

        [Required]
        [StringLength(50)]
        public string Tipo { get; set; } // "Fijo" o "Variable"

        [StringLength(250)]
        public string? Descripcion { get; set; }

        [Required]
        public decimal Monto { get; set; }

        // Relación con cuenta (opcional)
        public int? CuentaId { get; set; }
    }
}
