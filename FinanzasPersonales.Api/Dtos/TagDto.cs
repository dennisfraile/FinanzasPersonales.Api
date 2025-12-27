namespace FinanzasPersonales.Api.Dtos
{
    public class CreateTagDto
    {
        public string Nombre { get; set; } = string.Empty;
        public string Color { get; set; } = "#3b82f6";
    }

    public class UpdateTagDto
    {
        public string Nombre { get; set; } = string.Empty;
        public string Color { get; set; } = "#3b82f6";
    }

    public class TagDto
    {
        public int Id { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public string Color { get; set; } = string.Empty;
        public DateTime FechaCreacion { get; set; }
    }
}
