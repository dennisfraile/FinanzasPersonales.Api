using System.ComponentModel.DataAnnotations;

namespace FinanzasPersonales.Api.Dtos
{
    /// <summary>
    /// DTO para lectura de cuenta
    /// </summary>
    public class CuentaDto
    {
        public int Id { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public string Tipo { get; set; } = string.Empty;
        public decimal BalanceActual { get; set; }
        public decimal BalanceInicial { get; set; }
        public string Moneda { get; set; } = "MXN";
        public string? Color { get; set; }
        public string? Icono { get; set; }
        public bool Activa { get; set; }
        public DateTime FechaCreacion { get; set; }
    }

    /// <summary>
    /// DTO para crear cuenta
    /// </summary>
    public class CuentaCreateDto
    {
        [Required(ErrorMessage = "El nombre es requerido")]
        [StringLength(100, MinimumLength = 1, ErrorMessage = "El nombre debe tener entre 1 y 100 caracteres")]
        public string Nombre { get; set; } = string.Empty;

        [Required(ErrorMessage = "El tipo es requerido")]
        [RegularExpression("^(Efectivo|CuentaBancaria|TarjetaCredito|Ahorros|Inversion)$",
            ErrorMessage = "El tipo debe ser: Efectivo, CuentaBancaria, TarjetaCredito, Ahorros o Inversion")]
        public string Tipo { get; set; } = string.Empty;

        [Range(0, double.MaxValue, ErrorMessage = "El balance inicial no puede ser negativo")]
        public decimal BalanceInicial { get; set; }

        [StringLength(10)]
        public string Moneda { get; set; } = "MXN";

        [StringLength(20)]
        public string? Color { get; set; }

        [StringLength(50)]
        public string? Icono { get; set; }
    }

    /// <summary>
    /// DTO para actualizar cuenta
    /// </summary>
    public class CuentaUpdateDto
    {
        [Required(ErrorMessage = "El nombre es requerido")]
        [StringLength(100, MinimumLength = 1, ErrorMessage = "El nombre debe tener entre 1 y 100 caracteres")]
        public string Nombre { get; set; } = string.Empty;

        public decimal BalanceActual { get; set; }

        [StringLength(20)]
        public string? Color { get; set; }

        [StringLength(50)]
        public string? Icono { get; set; }

        public bool Activa { get; set; }
    }

    /// <summary>
    /// DTO para transferencia
    /// </summary>
    public class TransferenciaDto
    {
        public int Id { get; set; }
        public int CuentaOrigenId { get; set; }
        public string CuentaOrigenNombre { get; set; } = string.Empty;
        public int CuentaDestinoId { get; set; }
        public string CuentaDestinoNombre { get; set; } = string.Empty;
        public decimal Monto { get; set; }
        public DateTime Fecha { get; set; }
        public string? Descripcion { get; set; }
    }

    /// <summary>
    /// DTO para crear transferencia
    /// </summary>
    public class TransferenciaCreateDto
    {
        [Required(ErrorMessage = "La cuenta origen es requerida")]
        public int CuentaOrigenId { get; set; }

        [Required(ErrorMessage = "La cuenta destino es requerida")]
        public int CuentaDestinoId { get; set; }

        [Required(ErrorMessage = "El monto es requerido")]
        [Range(0.01, double.MaxValue, ErrorMessage = "El monto debe ser mayor a 0")]
        public decimal Monto { get; set; }

        [StringLength(500, ErrorMessage = "La descripción no puede exceder 500 caracteres")]
        public string? Descripcion { get; set; }
    }
}
