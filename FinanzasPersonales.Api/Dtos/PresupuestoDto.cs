namespace FinanzasPersonales.Api.Dtos
{
    /// <summary>
    /// DTO para mostrar información de un presupuesto con cálculos.
    /// </summary>
    public class PresupuestoDto
    {
        public int Id { get; set; }
        public int CategoriaId { get; set; }
        public required string CategoriaNombre { get; set; }
        public decimal MontoLimite { get; set; }
        public required string Periodo { get; set; }
        public int MesAplicable { get; set; }
        public int AnoAplicable { get; set; }

        // Calculados
        public decimal GastadoActual { get; set; }
        public decimal Disponible { get; set; }
        public decimal PorcentajeUtilizado { get; set; }
    }
}
