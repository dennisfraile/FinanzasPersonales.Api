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

        [StringLength(2000, ErrorMessage = "Las notas no pueden exceder 2000 caracteres")]
        public string? Notas { get; set; }

        // Tags asociados
        public List<int> TagIds { get; set; } = new List<int>();
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

        [StringLength(2000, ErrorMessage = "Las notas no pueden exceder 2000 caracteres")]
        public string? Notas { get; set; }

        // Tags asociados
        public List<int> TagIds { get; set; } = new List<int>();
    }

    /// <summary>
    /// DTO para respuesta GET de gastos
    /// </summary>
    public class GastoDto
    {
        public int Id { get; set; }
        public DateTime Fecha { get; set; }
        public int CategoriaId { get; set; }
        public string? CategoriaNombre { get; set; }
        public string? Tipo { get; set; }
        public string? Descripcion { get; set; }
        public decimal Monto { get; set; }
        public int? CuentaId { get; set; }
        public string? CuentaNombre { get; set; }
        public string? Notas { get; set; }
        public List<int> TagIds { get; set; } = new List<int>();
        public int CantidadDetalles { get; set; }
        public decimal? MontoDisponible { get; set; }

        /// <summary>
        /// Transferencias de saldo que afectan este gasto
        /// </summary>
        public List<TransferenciaGastoItemDto> Transferencias { get; set; } = new();
    }

    /// <summary>
    /// Transferencia de saldo individual en el contexto de un gasto
    /// </summary>
    public class TransferenciaGastoItemDto
    {
        public decimal Monto { get; set; }
        /// <summary>
        /// "entrada" o "salida" respecto al gasto actual
        /// </summary>
        public string Direccion { get; set; } = string.Empty;
        /// <summary>
        /// Descripción o categoría del otro gasto involucrado
        /// </summary>
        public string OtroGastoDescripcion { get; set; } = string.Empty;
        public DateTime Fecha { get; set; }
    }
}
