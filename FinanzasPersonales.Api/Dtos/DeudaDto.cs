using System.ComponentModel.DataAnnotations;

namespace FinanzasPersonales.Api.Dtos
{
    public class CreateDeudaDto
    {
        [Required(ErrorMessage = "El nombre es requerido")]
        [StringLength(200)]
        public string Nombre { get; set; } = string.Empty;

        [Required]
        [RegularExpression("^(TarjetaCredito|PrestamoPersonal|Hipoteca|PrestamoAuto|Otro)$")]
        public string Tipo { get; set; } = "Otro";

        [Required]
        [Range(0.01, double.MaxValue)]
        public decimal MontoOriginal { get; set; }

        [Required]
        [Range(0, double.MaxValue)]
        public decimal SaldoActual { get; set; }

        [Range(0, 100)]
        public decimal TasaInteres { get; set; }

        [Range(0.01, double.MaxValue)]
        public decimal? PagoMinimo { get; set; }

        [Range(1, 31)]
        public int? DiaDePago { get; set; }

        [Required]
        public DateTime FechaInicio { get; set; }

        public DateTime? FechaVencimiento { get; set; }

        public int? CuentaId { get; set; }

        [StringLength(500)]
        public string? Notas { get; set; }
    }

    public class UpdateDeudaDto
    {
        [Required]
        public int Id { get; set; }

        [Required(ErrorMessage = "El nombre es requerido")]
        [StringLength(200)]
        public string Nombre { get; set; } = string.Empty;

        [Required]
        [RegularExpression("^(TarjetaCredito|PrestamoPersonal|Hipoteca|PrestamoAuto|Otro)$")]
        public string Tipo { get; set; } = "Otro";

        [Range(0, 100)]
        public decimal TasaInteres { get; set; }

        [Range(0.01, double.MaxValue)]
        public decimal? PagoMinimo { get; set; }

        [Range(1, 31)]
        public int? DiaDePago { get; set; }

        public DateTime? FechaVencimiento { get; set; }

        public int? CuentaId { get; set; }

        public bool Activa { get; set; } = true;

        [StringLength(500)]
        public string? Notas { get; set; }
    }

    public class DeudaDto
    {
        public int Id { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public string Tipo { get; set; } = string.Empty;
        public decimal MontoOriginal { get; set; }
        public decimal SaldoActual { get; set; }
        public decimal TasaInteres { get; set; }
        public decimal? PagoMinimo { get; set; }
        public int? DiaDePago { get; set; }
        public DateTime FechaInicio { get; set; }
        public DateTime? FechaVencimiento { get; set; }
        public int? CuentaId { get; set; }
        public bool Activa { get; set; }
        public string? Notas { get; set; }
        public decimal TotalPagado { get; set; }
        public decimal PorcentajePagado { get; set; }
    }

    public class CreatePagoDeudaDto
    {
        [Required]
        [Range(0.01, double.MaxValue)]
        public decimal Monto { get; set; }

        [Required]
        public DateTime Fecha { get; set; }

        [StringLength(200)]
        public string? Descripcion { get; set; }
    }

    public class PagoDeudaDto
    {
        public int Id { get; set; }
        public int DeudaId { get; set; }
        public decimal Monto { get; set; }
        public decimal? MontoInteres { get; set; }
        public decimal? MontoCapital { get; set; }
        public DateTime Fecha { get; set; }
        public string? Descripcion { get; set; }
    }

    public class ProyeccionPagoDto
    {
        public int Mes { get; set; }
        public DateTime FechaPago { get; set; }
        public decimal PagoMensual { get; set; }
        public decimal InteresDelMes { get; set; }
        public decimal CapitalDelMes { get; set; }
        public decimal SaldoRestante { get; set; }
    }
}
