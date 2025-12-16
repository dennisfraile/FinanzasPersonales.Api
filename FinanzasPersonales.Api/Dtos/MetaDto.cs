using System.ComponentModel.DataAnnotations;

namespace FinanzasPersonales.Api.Dtos
{
    /// <summary>
    /// DTO para crear una nueva meta (sin UserId, lo asigna el backend)
    /// </summary>
    public class CreateMetaDto
    {
        [Required(ErrorMessage = "El nombre de la meta es requerido")]
        [StringLength(100, ErrorMessage = "El nombre no puede exceder 100 caracteres")]
        public string Metas { get; set; } = string.Empty;

        [Required(ErrorMessage = "El monto total es requerido")]
        [Range(0.01, double.MaxValue, ErrorMessage = "El monto debe ser mayor a 0")]
        public decimal MontoTotal { get; set; }

        [Range(0, double.MaxValue, ErrorMessage = "El ahorro actual no puede ser negativo")]
        public decimal AhorroActual { get; set; } = 0;

        [Range(0, double.MaxValue, ErrorMessage = "El monto restante no puede ser negativo")]
        public decimal MontoRestante { get; set; }
    }

    /// <summary>
    /// DTO para actualizar una meta existente
    /// </summary>
    public class UpdateMetaDto
    {
        [Required]
        public int Id { get; set; }

        [Required(ErrorMessage = "El nombre de la meta es requerido")]
        [StringLength(100, ErrorMessage = "El nombre no puede exceder 100 caracteres")]
        public string Metas { get; set; } = string.Empty;

        [Required(ErrorMessage = "El monto total es requerido")]
        [Range(0.01, double.MaxValue, ErrorMessage = "El monto debe ser mayor a 0")]
        public decimal MontoTotal { get; set; }

        [Range(0, double.MaxValue, ErrorMessage = "El ahorro actual no puede ser negativo")]
        public decimal AhorroActual { get; set; }

        [Range(0, double.MaxValue, ErrorMessage = "El monto restante no puede ser negativo")]
        public decimal MontoRestante { get; set; }
    }

    /// <summary>
    /// DTO para respuesta GET de metas financieras.
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
