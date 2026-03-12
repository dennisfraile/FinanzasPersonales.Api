using System.ComponentModel.DataAnnotations;

namespace FinanzasPersonales.Api.Dtos
{
    public class CreateTipoCambioDto
    {
        [Required]
        [StringLength(10)]
        public string MonedaOrigen { get; set; } = string.Empty;

        [Required]
        [StringLength(10)]
        public string MonedaDestino { get; set; } = string.Empty;

        [Required]
        [Range(0.000001, double.MaxValue)]
        public decimal Tasa { get; set; }
    }

    public class TipoCambioDto
    {
        public int Id { get; set; }
        public string MonedaOrigen { get; set; } = string.Empty;
        public string MonedaDestino { get; set; } = string.Empty;
        public decimal Tasa { get; set; }
        public DateTime Fecha { get; set; }
        public string Fuente { get; set; } = string.Empty;
    }

    public class ConversionDto
    {
        public decimal MontoOriginal { get; set; }
        public string MonedaOrigen { get; set; } = string.Empty;
        public decimal MontoConvertido { get; set; }
        public string MonedaDestino { get; set; } = string.Empty;
        public decimal TasaUsada { get; set; }
    }
}
