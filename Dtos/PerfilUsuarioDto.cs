namespace FinanzasPersonales.Api.Dtos
{
    /// <summary>
    /// DTO para información del perfil del usuario.
    /// </summary>
    public class PerfilUsuarioDto
    {
        public required string Email { get; set; }
        public DateTime FechaRegistro { get; set; }
        public int TotalCategorias { get; set; }
        public int TotalGastos { get; set; }
        public int TotalIngresos { get; set; }
        public int TotalMetas { get; set; }
    }
}
