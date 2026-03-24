using System.ComponentModel.DataAnnotations;

namespace FinanzasPersonales.Api.Dtos
{
    /// <summary>
    /// DTO para actualizar un presupuesto existente.
    /// </summary>
    public class PresupuestoUpdateDto
    {
        [Required]
        [Range(0.01, double.MaxValue, ErrorMessage = "El monto límite debe ser mayor a cero.")]
        public decimal MontoLimite { get; set; }

        [Required]
        [RegularExpression("^(Semanal|Quincenal|Mensual|Trimestral|Semestral|Anual)$", ErrorMessage = "El período debe ser 'Semanal', 'Quincenal', 'Mensual', 'Trimestral', 'Semestral' o 'Anual'.")]
        public required string Periodo { get; set; }

        public bool PermiteRollover { get; set; } = false;
    }
}
