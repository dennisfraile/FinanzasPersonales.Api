using System.ComponentModel.DataAnnotations;

namespace FinanzasPersonales.Api.Dtos
{
    public class CreateTagDto
    {
        [Required(ErrorMessage = "El nombre es requerido")]
        [StringLength(50, MinimumLength = 1, ErrorMessage = "El nombre debe tener entre 1 y 50 caracteres")]
        public string Nombre { get; set; } = string.Empty;

        [Required(ErrorMessage = "El color es requerido")]
        [RegularExpression("^#[0-9a-fA-F]{6}$", ErrorMessage = "El color debe ser un código hexadecimal válido (#RRGGBB)")]
        public string Color { get; set; } = "#3b82f6";
    }

    public class UpdateTagDto
    {
        [Required(ErrorMessage = "El nombre es requerido")]
        [StringLength(50, MinimumLength = 1, ErrorMessage = "El nombre debe tener entre 1 y 50 caracteres")]
        public string Nombre { get; set; } = string.Empty;

        [Required(ErrorMessage = "El color es requerido")]
        [RegularExpression("^#[0-9a-fA-F]{6}$", ErrorMessage = "El color debe ser un código hexadecimal válido (#RRGGBB)")]
        public string Color { get; set; } = "#3b82f6";
    }

    public class TagDto
    {
        public int Id { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public string Color { get; set; } = string.Empty;
        public DateTime FechaCreacion { get; set; }
    }
}
