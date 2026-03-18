using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FinanzasPersonales.Api.Models
{
    public class Categoria
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(100)]
        public string Nombre { get; set; } = null!;

        [Required]
        [StringLength(50)]
        public string Tipo { get; set; } = null!;

        // --- Vinculación con el Usuario ---
        [Required]
        public string UserId { get; set; } = null!;

        [ForeignKey("UserId")]
        public virtual IdentityUser User { get; set; } = null!;

        // Subcategorías
        public int? ParentCategoriaId { get; set; }

        [ForeignKey("ParentCategoriaId")]
        public virtual Categoria? ParentCategoria { get; set; }

        public virtual ICollection<Categoria> SubCategorias { get; set; } = new List<Categoria>();
    }
}
