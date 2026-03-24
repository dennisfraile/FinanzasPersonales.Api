namespace FinanzasPersonales.Api.Dtos
{
    public class CreateDashboardCompartidoDto
    {
        public string? NombreDestinatario { get; set; }
        public int? DiasExpiracion { get; set; }
        public string[]? Secciones { get; set; }
    }
}
