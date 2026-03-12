using System.ComponentModel.DataAnnotations;

namespace FinanzasPersonales.Api.Dtos
{
    public class CreateReglaCategoriaDto
    {
        [Required(ErrorMessage = "El patrón es requerido")]
        [StringLength(200)]
        public string Patron { get; set; } = string.Empty;

        [Required]
        [RegularExpression("^(Contiene|Exacto|ComienzaCon)$", ErrorMessage = "TipoCoincidencia debe ser 'Contiene', 'Exacto' o 'ComienzaCon'")]
        public string TipoCoincidencia { get; set; } = "Contiene";

        [Required(ErrorMessage = "La categoría es requerida")]
        public int CategoriaId { get; set; }

        [RegularExpression("^(Gasto|Ingreso|Ambos)$", ErrorMessage = "TipoTransaccion debe ser 'Gasto', 'Ingreso' o 'Ambos'")]
        public string TipoTransaccion { get; set; } = "Gasto";

        public int Prioridad { get; set; } = 0;
    }

    public class UpdateReglaCategoriaDto
    {
        [Required]
        public int Id { get; set; }

        [Required(ErrorMessage = "El patrón es requerido")]
        [StringLength(200)]
        public string Patron { get; set; } = string.Empty;

        [Required]
        [RegularExpression("^(Contiene|Exacto|ComienzaCon)$", ErrorMessage = "TipoCoincidencia debe ser 'Contiene', 'Exacto' o 'ComienzaCon'")]
        public string TipoCoincidencia { get; set; } = "Contiene";

        [Required(ErrorMessage = "La categoría es requerida")]
        public int CategoriaId { get; set; }

        [RegularExpression("^(Gasto|Ingreso|Ambos)$", ErrorMessage = "TipoTransaccion debe ser 'Gasto', 'Ingreso' o 'Ambos'")]
        public string TipoTransaccion { get; set; } = "Gasto";

        public int Prioridad { get; set; } = 0;
        public bool Activa { get; set; } = true;
    }

    public class ReglaCategoriaDto
    {
        public int Id { get; set; }
        public string Patron { get; set; } = string.Empty;
        public string TipoCoincidencia { get; set; } = string.Empty;
        public int CategoriaId { get; set; }
        public string? CategoriaNombre { get; set; }
        public string TipoTransaccion { get; set; } = string.Empty;
        public int Prioridad { get; set; }
        public bool Activa { get; set; }
    }

    public class CategoriaSugeridaDto
    {
        public int CategoriaId { get; set; }
        public string CategoriaNombre { get; set; } = string.Empty;
        public int ReglaId { get; set; }
        public string PatronCoincidido { get; set; } = string.Empty;
    }
}
