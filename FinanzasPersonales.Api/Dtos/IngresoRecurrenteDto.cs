using System.ComponentModel.DataAnnotations;

namespace FinanzasPersonales.Api.Dtos
{
    public class CreateIngresoRecurrenteDto
    {
        [Required(ErrorMessage = "La descripcion es requerida")]
        [StringLength(200)]
        public string Descripcion { get; set; } = string.Empty;

        [Required(ErrorMessage = "La categoria es requerida")]
        public int CategoriaId { get; set; }

        [Required(ErrorMessage = "El monto es requerido")]
        [Range(0.01, double.MaxValue, ErrorMessage = "El monto debe ser mayor a 0")]
        public decimal Monto { get; set; }

        public int? CuentaId { get; set; }

        [Required(ErrorMessage = "La frecuencia es requerida")]
        [RegularExpression("^(Semanal|Quincenal|Mensual|Anual)$", ErrorMessage = "Frecuencia debe ser: Semanal, Quincenal, Mensual o Anual")]
        public string Frecuencia { get; set; } = "Mensual";

        [Required(ErrorMessage = "El dia de pago es requerido")]
        [Range(1, 31, ErrorMessage = "El dia debe estar entre 1 y 31")]
        public int DiaDePago { get; set; }
    }

    public class UpdateIngresoRecurrenteDto : CreateIngresoRecurrenteDto
    {
        public bool Activo { get; set; }
    }

    public class IngresoRecurrenteDto
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
