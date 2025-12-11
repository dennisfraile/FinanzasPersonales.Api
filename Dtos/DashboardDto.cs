namespace FinanzasPersonales.Api.Dtos
{
    /// <summary>
    /// DTO para dashboard completo con todos los widgets.
    /// </summary>
    public class DashboardDto
    {
        public required ResumenMesActualDto MesActual { get; set; }
        public required List<EvolucionMensualDto> UltimosSeisMeses { get; set; }
        public required List<GastoPorCategoriaDto> TopCategorias { get; set; }
        public required List<PresupuestoDto> PresupuestosActivos { get; set; }
        public List<MetaDto>? MetasActivas { get; set; }
    }
}
