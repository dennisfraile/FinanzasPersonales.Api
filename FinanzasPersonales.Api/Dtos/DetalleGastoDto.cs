using System.ComponentModel.DataAnnotations;

namespace FinanzasPersonales.Api.Dtos
{
    /// <summary>
    /// DTO de respuesta para un detalle de gasto (sub-compra).
    /// </summary>
    public class DetalleGastoDto
    {
        public int Id { get; set; }
        public int GastoId { get; set; }
        public string Descripcion { get; set; } = string.Empty;
        public decimal Monto { get; set; }
        public DateTime Fecha { get; set; }
        public string? Notas { get; set; }
    }

    /// <summary>
    /// DTO para crear un detalle de gasto.
    /// </summary>
    public class CreateDetalleGastoDto
    {
        [Required(ErrorMessage = "La descripción es requerida")]
        [StringLength(250, ErrorMessage = "La descripción no puede exceder 250 caracteres")]
        public string Descripcion { get; set; } = string.Empty;

        [Required(ErrorMessage = "El monto es requerido")]
        [Range(0.01, double.MaxValue, ErrorMessage = "El monto debe ser mayor a 0")]
        public decimal Monto { get; set; }

        [Required(ErrorMessage = "La fecha es requerida")]
        public DateTime Fecha { get; set; }

        [StringLength(500, ErrorMessage = "Las notas no pueden exceder 500 caracteres")]
        public string? Notas { get; set; }
    }

    /// <summary>
    /// DTO de respuesta con el gasto y todos sus detalles + montos calculados.
    /// </summary>
    public class GastoConDetallesDto : GastoDto
    {
        public List<DetalleGastoDto> Detalles { get; set; } = new();
        public decimal MontoConsumido { get; set; }
        public decimal MontoDisponible { get; set; }
    }
}
