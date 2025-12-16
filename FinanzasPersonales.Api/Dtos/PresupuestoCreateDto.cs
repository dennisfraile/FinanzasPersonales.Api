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
        [RegularExpression("^(Mensual|Quincenal)$", ErrorMessage = "El período debe ser 'Mensual' o 'Quincenal'.")]
        public required string Periodo { get; set; }

        [Required]
        [Range(1, 12, ErrorMessage = "El mes debe estar entre 1 y 12.")]
        public int MesAplicable { get; set; }

        [Required]
        [Range(2020, 2100, ErrorMessage = "Año inválido.")]
        public int AnoAplicable { get; set; }
    }
}
