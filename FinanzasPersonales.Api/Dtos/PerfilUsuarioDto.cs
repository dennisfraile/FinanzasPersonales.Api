namespace FinanzasPersonales.Api.Dtos
{
    /// <summary>
    /// DTO para informacion del perfil del usuario.
    /// </summary>
    public class PerfilUsuarioDto
    {
        public required string Email { get; set; }
        public string? NombreCompleto { get; set; }
        public string? FotoUrl { get; set; }
        public DateTime FechaRegistro { get; set; }
        public int TotalCategorias { get; set; }
        public int TotalGastos { get; set; }
        public int TotalIngresos { get; set; }
        public int TotalMetas { get; set; }
    }
}
