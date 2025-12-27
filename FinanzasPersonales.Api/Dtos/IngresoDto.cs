using System.ComponentModel.DataAnnotations;

namespace FinanzasPersonales.Api.Dtos
{
    /// <summary>
    /// DTO para crear un nuevo ingreso (sin UserId, lo asigna el backend)
    /// </summary>
    public class CreateIngresoDto
    {
        [Required(ErrorMessage = "La fecha es requerida")]
        public DateTime Fecha { get; set; }

        [Required(ErrorMessage = "La categoría es requerida")]
        public int CategoriaId { get; set; }

        [StringLength(500, ErrorMessage = "La descripción no puede exceder 500 caracteres")]
        public string? Descripcion { get; set; }

        [Required(ErrorMessage = "El monto es requerido")]
        [Range(0.01, double.MaxValue, ErrorMessage = "El monto debe ser mayor a 0")]
        public decimal Monto { get; set; }

        // Relación con cuenta (opcional)
        public int? CuentaId { get; set; }
        public List<int> TagIds { get; set; } = new List<int>();
    }

    /// <summary>
    /// DTO para actualizar un ingreso existente
    /// </summary>
    public class UpdateIngresoDto
    {
        [Required]
        public int Id { get; set; }

        [Required(ErrorMessage = "La fecha es requerida")]
        public DateTime Fecha { get; set; }

        [Required(ErrorMessage = "La categoría es requerida")]
        public int CategoriaId { get; set; }

        [StringLength(500, ErrorMessage = "La descripción no puede exceder 500 caracteres")]
        public string? Descripcion { get; set; }

        [Required(ErrorMessage = "El monto es requerido")]
        [Range(0.01, double.MaxValue, ErrorMessage = "El monto debe ser mayor a 0")]
        public decimal Monto { get; set; }

        // Relación con cuenta (opcional)
        public int? CuentaId { get; set; }

        // Tags asociados  
        public List<int> TagIds { get; set; } = new List<int>();
    }

    /// <summary>
    /// DTO para respuesta GET de ingresos
    /// </summary>
    public class IngresoDto
    {
        public int Id { get; set; }
        public DateTime Fecha { get; set; }
        public int CategoriaId { get; set; }
        public string? CategoriaNombre { get; set; }
        public string? Descripcion { get; set; }
        public decimal Monto { get; set; }
        public int? CuentaId { get; set; }
        public List<int> TagIds { get; set; } = new List<int>();
    }
}
