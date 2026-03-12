using System.ComponentModel.DataAnnotations;

namespace FinanzasPersonales.Api.Dtos
{
    public class CreateReporteProgramadoDto
    {
        [Required]
        [RegularExpression("^(Semanal|Mensual)$", ErrorMessage = "Frecuencia debe ser 'Semanal' o 'Mensual'.")]
        public string Frecuencia { get; set; } = "Mensual";

        [Required]
        [EmailAddress]
        [StringLength(250)]
        public string EmailDestino { get; set; } = string.Empty;

        [Required]
        [MinLength(1)]
        public List<string> SeccionesIncluir { get; set; } = new() { "gastos", "ingresos" };
    }

    public class UpdateReporteProgramadoDto
    {
        [RegularExpression("^(Semanal|Mensual)$", ErrorMessage = "Frecuencia debe ser 'Semanal' o 'Mensual'.")]
        public string? Frecuencia { get; set; }

        [EmailAddress]
        [StringLength(250)]
        public string? EmailDestino { get; set; }

        public List<string>? SeccionesIncluir { get; set; }

        public bool? Activo { get; set; }
    }

    public class ReporteProgramadoDto
    {
        public int Id { get; set; }
        public string Frecuencia { get; set; } = string.Empty;
        public string EmailDestino { get; set; } = string.Empty;
        public List<string> SeccionesIncluir { get; set; } = new();
        public bool Activo { get; set; }
        public DateTime? UltimoEnvio { get; set; }
        public DateTime FechaCreacion { get; set; }
    }
}
