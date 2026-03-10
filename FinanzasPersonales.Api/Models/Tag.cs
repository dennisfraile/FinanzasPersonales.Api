using System.ComponentModel.DataAnnotations;

namespace FinanzasPersonales.Api.Models
{
    /// <summary>
    /// Etiqueta/Tag personalizable para organizar transacciones
    /// </summary>
    public class Tag
    {
        public int Id { get; set; }

        [Required]
        [MaxLength(50)]
        public string Nombre { get; set; } = string.Empty;

        [MaxLength(7)] // #RRGGBB format
        public string Color { get; set; } = "#3b82f6"; // Blue por defecto

        [Required]
        public string UserId { get; set; } = string.Empty;

        public DateTime FechaCreacion { get; set; } = DateTime.UtcNow;

        // Navigation properties para many-to-many
        public ICollection<GastoTag> GastoTags { get; set; } = new List<GastoTag>();
        public ICollection<IngresoTag> IngresoTags { get; set; } = new List<IngresoTag>();
    }

    /// <summary>
    /// Tabla intermedia para relación many-to-many entre Gasto y Tag
    /// </summary>
    public class GastoTag
    {
        public int GastoId { get; set; }
        public Gasto Gasto { get; set; } = null!;

        public int TagId { get; set; }
        public Tag Tag { get; set; } = null!;
    }

    /// <summary>
    /// Tabla intermedia para relación many-to-many entre Ingreso y Tag
    /// </summary>
    public class IngresoTag
    {
        public int IngresoId { get; set; }
        public Ingreso Ingreso { get; set; } = null!;

        public int TagId { get; set; }
        public Tag Tag { get; set; } = null!;
    }
}
