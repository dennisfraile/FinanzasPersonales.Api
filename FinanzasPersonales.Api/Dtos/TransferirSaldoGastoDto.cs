using System.ComponentModel.DataAnnotations;

namespace FinanzasPersonales.Api.Dtos
{
    public class TransferirSaldoGastoDto
    {
        [Required]
        public int GastoOrigenId { get; set; }

        [Required]
        public int GastoDestinoId { get; set; }

        [Required]
        [Range(0.01, (double)decimal.MaxValue, ErrorMessage = "El monto debe ser mayor a 0")]
        public decimal Monto { get; set; }
    }
}
