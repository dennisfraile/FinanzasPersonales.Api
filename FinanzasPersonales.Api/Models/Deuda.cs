using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Identity;

namespace FinanzasPersonales.Api.Models
{
    public class Deuda
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string UserId { get; set; } = string.Empty;

        [ForeignKey("UserId")]
        public virtual IdentityUser? User { get; set; }

        [Required]
        [StringLength(200)]
        public string Nombre { get; set; } = string.Empty;

        [Required]
        [StringLength(50)]
        public string Tipo { get; set; } = "Otro"; // TarjetaCredito, PrestamoPersonal, Hipoteca, PrestamoAuto, Otro

        [Required]
        [Column(TypeName = "decimal(18, 2)")]
        public decimal MontoOriginal { get; set; }

        [Required]
        [Column(TypeName = "decimal(18, 2)")]
        public decimal SaldoActual { get; set; }

        [Column(TypeName = "decimal(5, 2)")]
        public decimal TasaInteres { get; set; }

        [Column(TypeName = "decimal(18, 2)")]
        public decimal? PagoMinimo { get; set; }

        public int? DiaDePago { get; set; }

        public DateTime FechaInicio { get; set; }
        public DateTime? FechaVencimiento { get; set; }

        public int? CuentaId { get; set; }

        [ForeignKey("CuentaId")]
        public virtual Cuenta? Cuenta { get; set; }

        public bool Activa { get; set; } = true;

        [StringLength(500)]
        public string? Notas { get; set; }

        public DateTime FechaCreacion { get; set; } = DateTime.UtcNow;

        public virtual ICollection<PagoDeuda> Pagos { get; set; } = new List<PagoDeuda>();
    }

    public class PagoDeuda
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int DeudaId { get; set; }

        [ForeignKey("DeudaId")]
        public virtual Deuda? Deuda { get; set; }

        [Required]
        public string UserId { get; set; } = string.Empty;

        [Required]
        [Column(TypeName = "decimal(18, 2)")]
        public decimal Monto { get; set; }

        [Column(TypeName = "decimal(18, 2)")]
        public decimal? MontoInteres { get; set; }

        [Column(TypeName = "decimal(18, 2)")]
        public decimal? MontoCapital { get; set; }

        [Required]
        public DateTime Fecha { get; set; }

        [StringLength(200)]
        public string? Descripcion { get; set; }
    }
}
