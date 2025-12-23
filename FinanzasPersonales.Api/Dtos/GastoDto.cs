using System.ComponentModel.DataAnnotations;

namespace FinanzasPersonales.Api.Dtos
{
    /// <summary>
    /// DTO para crear un nuevo gasto (sin UserId, lo asigna el backend)
    /// </summary>
    public class CreateGastoDto
    {
        [Required(ErrorMessage = "La fecha es requerida")]
        public DateTime Fecha { get; set; }

        [Required(ErrorMessage = "La categoría es requerida")]
        public int CategoriaId { get; set; }

        [StringLength(20, ErrorMessage = "El tipo no puede exceder 20 caracteres")]
        public string? Tipo { get; set; }

        [StringLength(500, ErrorMessage = "La descripción no puede exceder 500 caracteres")]
        public string? Descripcion { get; set; }

        [Required(ErrorMessage = "El monto es requerido")]
        [Range(0.01, double.MaxValue, ErrorMessage = "El monto debe ser mayor a 0")]
        public decimal Monto { get; set; }

        // Relación con cuenta (opcional)
        public int? CuentaId { get; set; }
    }

    /// <summary>
    /// DTO para actualizar un gasto existente
    /// </summary>
    public class UpdateGastoDto
    {
        [Required]
        public int Id { get; set; }

        [Required(ErrorMessage = "La fecha es requerida")]
        public DateTime Fecha { get; set; }

        [Required(ErrorMessage = "La categoría es requerida")]
        public int CategoriaId { get; set; }

        [StringLength(20, ErrorMessage = "El tipo no puede exceder 20 caracteres")]
        public string? Tipo { get; set; }

        [StringLength(500, ErrorMessage = "La descripción no puede exceder 500 caracteres")]
        public string? Descripcion { get; set; }

        [Required(ErrorMessage = "El monto es requerido")]
        [Range(0.01, double.MaxValue, ErrorMessage = "El monto debe ser mayor a 0")]
        public decimal Monto { get; set; }

        // Relación con cuenta (opcional)
        public int? CuentaId { get; set; }
    }

    /// <summary>
    /// DTO para respuesta GET de gastos
    /// </summary>
    public class GastoDto
    {
        public int Id { get; set; }
        public DateTime Fecha { get; set; }
        public int CategoriaId { get; set; }
        public string CategoriaNombre { get; set; }
        public string Tipo { get; set; }
        public string? Descripcion { get; set; }
        public decimal Monto { get; set; }
    }
}
