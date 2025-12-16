using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Identity;

namespace FinanzasPersonales.Api.Models
{
    /// <summary>
    /// Tipos de cuenta bancaria
    /// </summary>
    public enum TipoCuenta
    {
        Efectivo,
        CuentaBancaria,
        TarjetaCredito,
        Ahorros,
        Inversion
    }

    /// <summary>
    /// Representa una cuenta financiera del usuario
    /// </summary>
    public class Cuenta
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public required string UserId { get; set; }

        [ForeignKey("UserId")]
        public virtual IdentityUser? User { get; set; }

        [Required]
        [StringLength(100)]
        public required string Nombre { get; set; }

        [Required]
        public TipoCuenta Tipo { get; set; }

        [Column(TypeName = "decimal(18, 2)")]
        public decimal BalanceActual { get; set; }

        [Column(TypeName = "decimal(18, 2)")]
        public decimal BalanceInicial { get; set; }

        [StringLength(10)]
        public string Moneda { get; set; } = "MXN";

        [StringLength(20)]
        public string? Color { get; set; }

        [StringLength(50)]
        public string? Icono { get; set; }

        public bool Activa { get; set; } = true;

        public DateTime FechaCreacion { get; set; } = DateTime.UtcNow;

        // Relaciones
        public virtual ICollection<Gasto> Gastos { get; set; } = new List<Gasto>();
        public virtual ICollection<Ingreso> Ingresos { get; set; } = new List<Ingreso>();
        public virtual ICollection<Transferencia> TransferenciasOrigen { get; set; } = new List<Transferencia>();
        public virtual ICollection<Transferencia> TransferenciasDestino { get; set; } = new List<Transferencia>();
    }

    /// <summary>
    /// Representa una transferencia entre dos cuentas del mismo usuario
    /// </summary>
    public class Transferencia
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public required string UserId { get; set; }

        [ForeignKey("UserId")]
        public virtual IdentityUser? User { get; set; }

        [Required]
        public int CuentaOrigenId { get; set; }

        [ForeignKey("CuentaOrigenId")]
        public virtual Cuenta? CuentaOrigen { get; set; }

        [Required]
        public int CuentaDestinoId { get; set; }

        [ForeignKey("CuentaDestinoId")]
        public virtual Cuenta? CuentaDestino { get; set; }

        [Required]
        [Column(TypeName = "decimal(18, 2)")]
        public decimal Monto { get; set; }

        [Required]
        public DateTime Fecha { get; set; } = DateTime.UtcNow;

        [StringLength(500)]
        public string? Descripcion { get; set; }
    }
}
