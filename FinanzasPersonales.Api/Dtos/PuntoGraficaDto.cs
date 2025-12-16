namespace FinanzasPersonales.Api.Dtos
{
    /// <summary>
    /// DTO para representar un punto en una gráfica.
    /// </summary>
    public class PuntoGraficaDto
    {
        public required string Etiqueta { get; set; }
        public decimal Valor { get; set; }
        public string? Color { get; set; } // Color hex para frontend (ej: "#FF5733")
    }
}
