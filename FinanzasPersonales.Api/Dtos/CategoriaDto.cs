using System.ComponentModel.DataAnnotations;

namespace FinanzasPersonales.Api.Dtos
{
    /// <summary>
    /// DTO para crear una nueva categoría sin requerir UserId (se asigna automáticamente)
    /// </summary>
    public class CreateCategoriaDto
    {
        [Required(ErrorMessage = "El nombre es requerido")]
        [StringLength(100, MinimumLength = 1, ErrorMessage = "El nombre debe tener entre 1 y 100 caracteres")]
        public string Nombre { get; set; } = string.Empty;

        [Required(ErrorMessage = "El tipo es requerido")]
        [RegularExpression("^(Ingreso|Gasto)$", ErrorMessage = "El tipo debe ser 'Ingreso' o 'Gasto'")]
        public string Tipo { get; set; } = string.Empty;

        public int? ParentCategoriaId { get; set; }
    }

    /// <summary>
    /// DTO para actualizar una categoría existente
    /// </summary>
    public class UpdateCategoriaDto
    {
        [Required]
        public int Id { get; set; }

        [Required(ErrorMessage = "El nombre es requerido")]
        [StringLength(100, MinimumLength = 1, ErrorMessage = "El nombre debe tener entre 1 y 100 caracteres")]
        public string Nombre { get; set; } = string.Empty;

        [Required(ErrorMessage = "El tipo es requerido")]
        [RegularExpression("^(Ingreso|Gasto)$", ErrorMessage = "El tipo debe ser 'Ingreso' o 'Gasto'")]
        public string Tipo { get; set; } = string.Empty;

        public int? ParentCategoriaId { get; set; }
    }

    /// <summary>
    /// DTO de respuesta con información jerárquica
    /// </summary>
    public class CategoriaArbolDto
    {
        public int Id { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public string Tipo { get; set; } = string.Empty;
        public int? ParentCategoriaId { get; set; }
        public string? ParentCategoriaNombre { get; set; }
        public List<CategoriaArbolDto> SubCategorias { get; set; } = new();
    }
}
