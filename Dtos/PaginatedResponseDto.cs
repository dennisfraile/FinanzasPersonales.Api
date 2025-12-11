namespace FinanzasPersonales.Api.Dtos
{
    /// <summary>
    /// DTO genérico para respuestas paginadas.
    /// </summary>
    public class PaginatedResponseDto<T>
    {
        public required List<T> Items { get; set; }
        public int PaginaActual { get; set; }
        public int TamañoPagina { get; set; }
        public int TotalItems { get; set; }
        public int TotalPaginas { get; set; }
        public bool TienePaginaAnterior { get; set; }
        public bool TienePaginaSiguiente { get; set; }
    }
}
