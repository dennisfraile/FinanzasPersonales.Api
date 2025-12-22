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
        public string Nombre { get; set; } = string.Empty;
        public string Tipo { get; set; } = string.Empty;  // Efectivo, CuentaBancaria, TarjetaCredito, Ahorros, Inversion
        public decimal BalanceInicial { get; set; }
        public string Moneda { get; set; } = "MXN";
        public string? Color { get; set; }
        public string? Icono { get; set; }
    }

    /// <summary>
    /// DTO para actualizar cuenta
    /// </summary>
    public class CuentaUpdateDto
    {
        public string Nombre { get; set; } = string.Empty;
        public decimal BalanceActual { get; set; }
        public string? Color { get; set; }
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
        public int CuentaOrigenId { get; set; }
        public int CuentaDestinoId { get; set; }
        public decimal Monto { get; set; }
        public string? Descripcion { get; set; }
    }
}
