using System.ComponentModel.DataAnnotations;

namespace FinanzasPersonales.Api.Dtos
{
    public class CsvColumnMappingDto
    {
        [Required]
        public int ColumnaFecha { get; set; }

        [Required]
        public int ColumnaMonto { get; set; }

        public int? ColumnaDescripcion { get; set; }

        [Required]
        public string FormatoFecha { get; set; } = "yyyy-MM-dd";

        public bool MontoNegativoEsGasto { get; set; } = true;
    }

    public class CsvImportRequestDto
    {
        [Required]
        public int CuentaId { get; set; }

        [Required]
        public CsvColumnMappingDto Mapeo { get; set; } = null!;

        public int? CategoriaIdDefault { get; set; }
        public bool PrimeraFilaEsEncabezado { get; set; } = true;
    }

    public class CsvPreviewRowDto
    {
        public int Fila { get; set; }
        public DateTime? Fecha { get; set; }
        public decimal? Monto { get; set; }
        public string? Descripcion { get; set; }
        public string? TipoDetectado { get; set; }
        public bool EsDuplicado { get; set; }
        public int? CategoriaIdSugerida { get; set; }
        public string? CategoriaNombreSugerida { get; set; }
        public string? Error { get; set; }
    }

    public class CsvPreviewResponseDto
    {
        public List<string[]> PrimerasFilas { get; set; } = new();
        public int TotalFilas { get; set; }
        public List<string> Columnas { get; set; } = new();
    }

    public class CsvImportResultDto
    {
        public int ImportacionId { get; set; }
        public int TotalFilas { get; set; }
        public int FilasImportadas { get; set; }
        public int FilasDuplicadas { get; set; }
        public int FilasError { get; set; }
        public List<CsvImportErrorDto> Errores { get; set; } = new();
    }

    public class CsvImportErrorDto
    {
        public int Fila { get; set; }
        public string Mensaje { get; set; } = string.Empty;
    }
}
