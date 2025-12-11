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
        [RegularExpression("^(Mensual|Quincenal)$", ErrorMessage = "El período debe ser 'Mensual' o 'Quincenal'.")]
        public required string Periodo { get; set; }
    }
}
