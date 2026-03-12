using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Identity;

namespace FinanzasPersonales.Api.Models
{
    public class ReglaCategoriaAutomatica
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string UserId { get; set; } = string.Empty;

        [ForeignKey("UserId")]
        public virtual IdentityUser? User { get; set; }

        [Required]
        [StringLength(200)]
        public string Patron { get; set; } = string.Empty;

        [Required]
        [StringLength(20)]
        public string TipoCoincidencia { get; set; } = "Contiene"; // Contiene, Exacto, ComienzaCon

        [Required]
        public int CategoriaId { get; set; }

        [ForeignKey("CategoriaId")]
        public virtual Categoria? Categoria { get; set; }

        [StringLength(10)]
        public string TipoTransaccion { get; set; } = "Gasto"; // Gasto, Ingreso, Ambos

        public int Prioridad { get; set; } = 0;

        public bool Activa { get; set; } = true;

        public DateTime FechaCreacion { get; set; } = DateTime.UtcNow;
    }
}
