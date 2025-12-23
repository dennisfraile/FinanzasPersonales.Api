using System.ComponentModel.DataAnnotations;

namespace FinanzasPersonales.Api.Models
{
    /// <summary>
    /// Modelo para adjuntos (comprobantes, facturas) asociados a transacciones
    /// </summary>
    public class Adjunto
    {
        public int Id { get; set; }

        [Required]
        [StringLength(255)]
        public string FileName { get; set; } = string.Empty;

        [Required]
        [StringLength(500)]
        public string FilePath { get; set; } = string.Empty;

        [Required]
        [StringLength(100)]
        public string ContentType { get; set; } = string.Empty;

        [Required]
        public long FileSize { get; set; }

        // Relación polimórfica - puede ser de un Gasto O un Ingreso
        public int? GastoId { get; set; }
        public Gasto? Gasto { get; set; }

        public int? IngresoId { get; set; }
        public Ingreso? Ingreso { get; set; }

        [Required]
        public string UserId { get; set; } = string.Empty;

        public DateTime FechaSubida { get; set; } = DateTime.UtcNow;
    }
}
