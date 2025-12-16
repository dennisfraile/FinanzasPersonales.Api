namespace FinanzasPersonales.Api.Dtos
{
    /// <summary>
    /// DTO para crear una nueva categoría sin requerir UserId (se asigna automáticamente)
    /// </summary>
    public class CreateCategoriaDto
    {
        public string Nombre { get; set; }
        public string Tipo { get; set; } // "Ingreso" o "Gasto"
    }

    /// <summary>
    /// DTO para actualizar una categoría existente
    /// </summary>
    public class UpdateCategoriaDto
    {
        public int Id { get; set; }
        public string Nombre { get; set; }
        public string Tipo { get; set; }
    }
}
