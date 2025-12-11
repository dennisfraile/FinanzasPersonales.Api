namespace FinanzasPersonales.Api.Dtos
{
    /// <summary>
    /// DTO para metas financieras.
    /// </summary>
    public class MetaDto
    {
        public int Id { get; set; }
        public required string Metas { get; set; }
        public decimal MontoTotal { get; set; }
        public decimal AhorroActual { get; set; }
        public decimal MontoRestante { get; set; }
        public decimal PorcentajeProgreso { get; set; }
    }
}
