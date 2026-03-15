using System.ComponentModel.DataAnnotations;

namespace FinanzasPersonales.Api.Dtos
{
    /// <summary>
    /// DTO para crear un nuevo presupuesto.
    /// </summary>
    public class PresupuestoCreateDto
    {
        [Required]
        public int CategoriaId { get; set; }

        [Required]
        [Range(0.01, double.MaxValue, ErrorMessage = "El monto límite debe ser mayor a cero.")]
        public decimal MontoLimite { get; set; }

        [Required]
        [RegularExpression("^(Semanal|Quincenal|Mensual|Trimestral|Semestral|Anual)$", ErrorMessage = "El período debe ser 'Semanal', 'Quincenal', 'Mensual', 'Trimestral', 'Semestral' o 'Anual'.")]
        public required string Periodo { get; set; }

        [Required]
        [Range(1, 12, ErrorMessage = "El mes debe estar entre 1 y 12.")]
        public int MesAplicable { get; set; }

        [Required]
        [Range(2020, 2100, ErrorMessage = "Año inválido.")]
        public int AnoAplicable { get; set; }

        [Range(1, 53, ErrorMessage = "La semana debe estar entre 1 y 53.")]
        public int? SemanaAplicable { get; set; }
    }
}
