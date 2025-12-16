namespace FinanzasPersonales.Api.Dtos
{
    /// <summary>
    /// DTO para datos de gráfica con título y puntos.
    /// </summary>
    public class GraficaDto
    {
        public required string Titulo { get; set; }
        public required List<PuntoGraficaDto> Datos { get; set; }
    }
}
