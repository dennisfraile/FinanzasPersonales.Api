namespace FinanzasPersonales.Api.Dtos
{
    /// <summary>
    /// DTO para reportes de gastos agrupados por categoría.
    /// </summary>
    public class GastoPorCategoriaDto
    {
        public int CategoriaId { get; set; }
        public required string CategoriaNombre { get; set; }
        public decimal TotalGastado { get; set; }
        public decimal PorcentajeDelTotal { get; set; }
        public int CantidadTransacciones { get; set; }
    }
}
