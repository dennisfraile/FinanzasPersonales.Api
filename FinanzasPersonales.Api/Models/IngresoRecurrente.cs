using System.ComponentModel.DataAnnotations;

namespace FinanzasPersonales.Api.Models
{
    public class IngresoRecurrente
    {
        public int Id { get; set; }

        [Required]
        public string UserId { get; set; } = string.Empty;

        [Required]
        [StringLength(200)]
        public string Descripcion { get; set; } = string.Empty;

        [Required]
        public int CategoriaId { get; set; }
        public Categoria? Categoria { get; set; }

        [Required]
        [Range(0.01, double.MaxValue, ErrorMessage = "El monto debe ser mayor a 0")]
        public decimal Monto { get; set; }

        public int? CuentaId { get; set; }
        public Cuenta? Cuenta { get; set; }

        [Required]
        [StringLength(20)]
        public string Frecuencia { get; set; } = "Mensual"; // Semanal, Quincenal, Mensual, Anual

        [Required]
        [Range(1, 31)]
        public int DiaDePago { get; set; }

        [Required]
        public DateTime ProximaFecha { get; set; }

        public DateTime? UltimaGeneracion { get; set; }

        public bool Activo { get; set; } = true;

        public DateTime FechaCreacion { get; set; } = DateTime.UtcNow;
    }
}
