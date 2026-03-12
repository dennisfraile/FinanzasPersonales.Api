using System.ComponentModel.DataAnnotations;

namespace FinanzasPersonales.Api.Dtos
{
    public class CreateGastoCompartidoDto
    {
        [Required]
        [StringLength(200)]
        public string Descripcion { get; set; } = string.Empty;

        [Required]
        [Range(0.01, double.MaxValue)]
        public decimal MontoTotal { get; set; }

        [Required]
        public DateTime Fecha { get; set; }

        public int? CategoriaId { get; set; }

        [RegularExpression("^(Equitativo|Porcentaje|MontoFijo)$")]
        public string MetodoDivision { get; set; } = "Equitativo";

        [Required]
        public List<CreateParticipanteDto> Participantes { get; set; } = new();
    }

    public class CreateParticipanteDto
    {
        [Required]
        [StringLength(100)]
        public string Nombre { get; set; } = string.Empty;

        [StringLength(200)]
        public string? Email { get; set; }

        public decimal? MontoAsignado { get; set; }
        public decimal? Porcentaje { get; set; }
    }

    public class GastoCompartidoDto
    {
        public int Id { get; set; }
        public string Descripcion { get; set; } = string.Empty;
        public decimal MontoTotal { get; set; }
        public DateTime Fecha { get; set; }
        public int? CategoriaId { get; set; }
        public string? CategoriaNombre { get; set; }
        public string MetodoDivision { get; set; } = string.Empty;
        public decimal MontoRecuperado { get; set; }
        public decimal MontoPendiente { get; set; }
        public List<ParticipanteGastoDto> Participantes { get; set; } = new();
    }

    public class ParticipanteGastoDto
    {
        public int Id { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public string? Email { get; set; }
        public decimal MontoAsignado { get; set; }
        public decimal MontoPagado { get; set; }
        public bool Liquidado { get; set; }
        public DateTime? FechaLiquidacion { get; set; }
    }

    public class LiquidarParticipanteDto
    {
        [Required]
        [Range(0.01, double.MaxValue)]
        public decimal Monto { get; set; }
    }

    public class ResumenSplitDto
    {
        public decimal TotalPendientePorCobrar { get; set; }
        public decimal TotalRecuperado { get; set; }
        public List<DeudorResumenDto> Deudores { get; set; } = new();
    }

    public class DeudorResumenDto
    {
        public string Nombre { get; set; } = string.Empty;
        public decimal TotalDeuda { get; set; }
        public decimal TotalPagado { get; set; }
        public decimal Pendiente { get; set; }
    }
}
