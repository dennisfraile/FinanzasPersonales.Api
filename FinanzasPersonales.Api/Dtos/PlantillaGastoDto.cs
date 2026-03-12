using System.ComponentModel.DataAnnotations;

namespace FinanzasPersonales.Api.Dtos
{
    public class CreatePlantillaGastoDto
    {
        [Required(ErrorMessage = "El nombre es requerido")]
        [StringLength(100)]
        public string Nombre { get; set; } = string.Empty;

        [Required(ErrorMessage = "La categoría es requerida")]
        public int CategoriaId { get; set; }

        [Range(0.01, double.MaxValue, ErrorMessage = "El monto debe ser mayor a 0")]
        public decimal? Monto { get; set; }

        [StringLength(250)]
        public string? Descripcion { get; set; }

        [StringLength(50)]
        public string? Tipo { get; set; } = "Variable";

        public int? CuentaId { get; set; }

        [StringLength(50)]
        public string? Icono { get; set; }

        [StringLength(20)]
        public string? Color { get; set; }

        public int OrdenDisplay { get; set; } = 0;
    }

    public class UpdatePlantillaGastoDto
    {
        [Required]
        public int Id { get; set; }

        [Required(ErrorMessage = "El nombre es requerido")]
        [StringLength(100)]
        public string Nombre { get; set; } = string.Empty;

        [Required(ErrorMessage = "La categoría es requerida")]
        public int CategoriaId { get; set; }

        [Range(0.01, double.MaxValue, ErrorMessage = "El monto debe ser mayor a 0")]
        public decimal? Monto { get; set; }

        [StringLength(250)]
        public string? Descripcion { get; set; }

        [StringLength(50)]
        public string? Tipo { get; set; } = "Variable";

        public int? CuentaId { get; set; }

        [StringLength(50)]
        public string? Icono { get; set; }

        [StringLength(20)]
        public string? Color { get; set; }

        public int OrdenDisplay { get; set; } = 0;
    }

    public class PlantillaGastoDto
    {
        public int Id { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public int CategoriaId { get; set; }
        public string? CategoriaNombre { get; set; }
        public decimal? Monto { get; set; }
        public string? Descripcion { get; set; }
        public string? Tipo { get; set; }
        public int? CuentaId { get; set; }
        public string? Icono { get; set; }
        public string? Color { get; set; }
        public int OrdenDisplay { get; set; }
        public int VecesUsada { get; set; }
    }

    public class UsarPlantillaDto
    {
        public DateTime? Fecha { get; set; }

        [Range(0.01, double.MaxValue, ErrorMessage = "El monto debe ser mayor a 0")]
        public decimal? Monto { get; set; }
    }
}
