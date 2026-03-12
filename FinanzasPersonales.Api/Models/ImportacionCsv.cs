using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Identity;

namespace FinanzasPersonales.Api.Models
{
    public class ImportacionCsv
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string UserId { get; set; } = string.Empty;

        [ForeignKey("UserId")]
        public virtual IdentityUser? User { get; set; }

        [Required]
        [StringLength(200)]
        public string NombreArchivo { get; set; } = string.Empty;

        public int TotalFilas { get; set; }
        public int FilasImportadas { get; set; }
        public int FilasDuplicadas { get; set; }
        public int FilasError { get; set; }

        public DateTime FechaImportacion { get; set; } = DateTime.UtcNow;

        [StringLength(20)]
        public string Estado { get; set; } = "Completada"; // Completada, Parcial, Error
    }
}
