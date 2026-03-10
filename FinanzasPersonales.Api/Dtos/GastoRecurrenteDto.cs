using System.ComponentModel.DataAnnotations;

namespace FinanzasPersonales.Api.Dtos
{
    /// <summary>
    /// DTO para crear un gasto recurrente
    /// </summary>
    public class CreateGastoRecurrenteDto
    {
        [Required(ErrorMessage = "La descripción es requerida")]
        [StringLength(200)]
        public string Descripcion { get; set; } = string.Empty;

        [Required(ErrorMessage = "La categoría es requerida")]
        public int CategoriaId { get; set; }

        [Required(ErrorMessage = "El monto es requerido")]
        [Range(0.01, double.MaxValue, ErrorMessage = "El monto debe ser mayor a 0")]
        public decimal Monto { get; set; }

        public int? CuentaId { get; set; }

        [Required(ErrorMessage = "La frecuencia es requerida")]
        [RegularExpression("^(Semanal|Quincenal|Mensual|Anual)$", ErrorMessage = "Frecuencia debe ser: Semanal, Quincenal, Mensual o Anual")]
        public string Frecuencia { get; set; } = "Mensual";

        [Required(ErrorMessage = "El día de pago es requerido")]
        [Range(1, 31, ErrorMessage = "El día debe estar entre 1 y 31")]
        public int DiaDePago { get; set; }
    }

    /// <summary>
    /// DTO para actualizar un gasto recurrente
    /// </summary>
    public class UpdateGastoRecurrenteDto : CreateGastoRecurrenteDto
    {
        public bool Activo { get; set; }
    }

    /// <summary>
    /// DTO de respuesta con información completa
    /// </summary>
    public class GastoRecurrenteDto
    {
        public int Id { get; set; }
        public string Descripcion { get; set; } = string.Empty;
        public int CategoriaId { get; set; }
        public string? CategoriaNombre { get; set; }
        public decimal Monto { get; set; }
        public int? CuentaId { get; set; }
        public string? CuentaNombre { get; set; }
        public string Frecuencia { get; set; } = string.Empty;
        public int DiaDePago { get; set; }
        public DateTime ProximaFecha { get; set; }
        public DateTime? UltimaGeneracion { get; set; }
        public bool Activo { get; set; }
        public DateTime FechaCreacion { get; set; }
    }
}
