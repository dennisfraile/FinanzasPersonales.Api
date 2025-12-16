using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Identity;

namespace FinanzasPersonales.Api.Models
{
    /// <summary>
    /// Tipos de notificaciones del sistema
    /// </summary>
    public enum TipoNotificacion
    {
        PresupuestoAlerta,
        MetaCercana,
        GastoInusual,
        ResumenMensual,
        Informativa
    }

    /// <summary>
    /// Representa una notificación para el usuario.
    /// </summary>
    public class Notificacion
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public required string UserId { get; set; }

        [ForeignKey("UserId")]
        public virtual IdentityUser? User { get; set; }

        [Required]
        public TipoNotificacion Tipo { get; set; }

        [Required]
        [StringLength(200)]
        public required string Titulo { get; set; }

        [Required]
        [StringLength(1000)]
        public required string Mensaje { get; set; }

        [Required]
        public DateTime FechaCreacion { get; set; } = DateTime.UtcNow;

        public bool Leida { get; set; } = false;

        /// <summary>
        /// Datos adicionales en formato JSON
        /// </summary>
        public string? DatosAdicionales { get; set; }

        /// <summary>
        /// ID de referencia a presupuesto, meta o gasto
        /// </summary>
        public int? ReferenciaId { get; set; }

        public bool EmailEnviado { get; set; } = false;

        public DateTime? FechaEmailEnviado { get; set; }
    }

    /// <summary>
    /// Configuración de notificaciones del usuario
    /// </summary>
    public class ConfiguracionNotificaciones
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public required string UserId { get; set; }

        [ForeignKey("UserId")]
        public virtual IdentityUser? User { get; set; }

        public bool AlertasPresupuesto { get; set; } = true;
        public bool RecordatorioMetas { get; set; } = true;
        public bool GastosInusuales { get; set; } = true;
        public bool ResumenMensual { get; set; } = false;

        [Range(50, 100)]
        public int UmbralPresupuesto { get; set; } = 80;

        [Range(50, 100)]
        public int UmbralMeta { get; set; } = 90;

        [Range(1.5, 5.0)]
        [Column(TypeName = "decimal(3, 1)")]
        public decimal FactorGastoInusual { get; set; } = 2.0m;
    }
}
