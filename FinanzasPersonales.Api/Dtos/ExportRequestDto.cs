using System.ComponentModel.DataAnnotations;

namespace FinanzasPersonales.Api.Dtos
{
    /// <summary>
    /// DTO para solicitar exportación de datos.
    /// </summary>
    public class ExportRequestDto
    {
        /// <summary>
        /// Formato de exportación: "excel", "pdf", "json"
        /// </summary>
        [Required(ErrorMessage = "El formato es requerido.")]
        [RegularExpression("^(excel|pdf|json)$", ErrorMessage = "Formato debe ser: excel, pdf o json.")]
        public required string Formato { get; set; }

        /// <summary>
        /// Fecha inicial del rango (opcional, por defecto inicio de mes actual)
        /// </summary>
        public DateTime? Desde { get; set; }

        /// <summary>
        /// Fecha final del rango (opcional, por defecto fecha actual)
        /// </summary>
        public DateTime? Hasta { get; set; }

        /// <summary>
        /// Tipos de datos a incluir en la exportación
        /// </summary>
        public List<string>? Incluir { get; set; } // ["gastos", "ingresos", "metas", "presupuestos"]
    }
}
