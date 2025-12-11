namespace FinanzasPersonales.Api.Dtos
{
    public class GastoDto
    {
        public int Id { get; set; }
        public DateTime Fecha { get; set; }

        // --- ¡La solución para GET! ---
        public int CategoriaId { get; set; }
        public string CategoriaNombre { get; set; }

        public string Tipo { get; set; }
        public string? Descripcion { get; set; }
        public decimal Monto { get; set; }
    }
}
