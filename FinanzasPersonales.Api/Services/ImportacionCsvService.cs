using System.Globalization;
using CsvHelper;
using CsvHelper.Configuration;
using Microsoft.EntityFrameworkCore;
using FinanzasPersonales.Api.Data;
using FinanzasPersonales.Api.Dtos;
using FinanzasPersonales.Api.Models;

namespace FinanzasPersonales.Api.Services
{
    public interface IImportacionCsvService
    {
        Task<CsvPreviewResponseDto> PreviewCsvAsync(Stream csvStream);
        Task<List<CsvPreviewRowDto>> ValidateAndPreviewAsync(string userId, Stream csvStream, CsvImportRequestDto request);
        Task<CsvImportResultDto> ImportCsvAsync(string userId, Stream csvStream, CsvImportRequestDto request, string nombreArchivo);
    }

    public class ImportacionCsvService : IImportacionCsvService
    {
        private readonly FinanzasDbContext _context;
        private readonly IReglasCategoriaService _reglasService;

        public ImportacionCsvService(FinanzasDbContext context, IReglasCategoriaService reglasService)
        {
            _context = context;
            _reglasService = reglasService;
        }

        public async Task<CsvPreviewResponseDto> PreviewCsvAsync(Stream csvStream)
        {
            var rows = await ReadAllRowsAsync(csvStream);
            var response = new CsvPreviewResponseDto
            {
                TotalFilas = rows.Count
            };

            if (rows.Count > 0)
            {
                response.Columnas = rows[0].ToList();
                response.PrimerasFilas = rows.Take(6).ToList();
            }

            return response;
        }

        public async Task<List<CsvPreviewRowDto>> ValidateAndPreviewAsync(string userId, Stream csvStream, CsvImportRequestDto request)
        {
            var rows = await ReadAllRowsAsync(csvStream);
            var dataRows = request.PrimeraFilaEsEncabezado ? rows.Skip(1).ToList() : rows;
            var mapeo = request.Mapeo;
            var previews = new List<CsvPreviewRowDto>();

            for (int i = 0; i < Math.Min(dataRows.Count, 20); i++)
            {
                var row = dataRows[i];
                var preview = await ParseRowAsync(userId, row, i + 1, mapeo, request);
                previews.Add(preview);
            }

            return previews;
        }

        public async Task<CsvImportResultDto> ImportCsvAsync(string userId, Stream csvStream, CsvImportRequestDto request, string nombreArchivo)
        {
            // Validar que la cuenta pertenece al usuario
            var cuenta = await _context.Cuentas.FirstOrDefaultAsync(c => c.Id == request.CuentaId && c.UserId == userId);
            if (cuenta == null)
                throw new InvalidOperationException("Recurso no encontrado o acceso denegado.");

            var rows = await ReadAllRowsAsync(csvStream);
            var dataRows = request.PrimeraFilaEsEncabezado ? rows.Skip(1).ToList() : rows;
            var mapeo = request.Mapeo;

            var result = new CsvImportResultDto { TotalFilas = dataRows.Count };
            var importacion = new ImportacionCsv
            {
                UserId = userId,
                NombreArchivo = nombreArchivo,
                TotalFilas = dataRows.Count
            };

            for (int i = 0; i < dataRows.Count; i++)
            {
                var row = dataRows[i];
                var parsed = await ParseRowAsync(userId, row, i + 1, mapeo, request);

                if (parsed.Error != null)
                {
                    result.FilasError++;
                    result.Errores.Add(new CsvImportErrorDto { Fila = i + 1, Mensaje = parsed.Error });
                    continue;
                }

                if (parsed.EsDuplicado)
                {
                    result.FilasDuplicadas++;
                    continue;
                }

                var esGasto = parsed.TipoDetectado == "Gasto";
                var categoriaId = parsed.CategoriaIdSugerida ?? request.CategoriaIdDefault;

                if (!categoriaId.HasValue)
                {
                    result.FilasError++;
                    result.Errores.Add(new CsvImportErrorDto { Fila = i + 1, Mensaje = "No se pudo determinar la categoría." });
                    continue;
                }

                var fecha = DateTime.SpecifyKind(parsed.Fecha!.Value, DateTimeKind.Utc);
                var monto = Math.Abs(parsed.Monto!.Value);

                if (esGasto)
                {
                    var gasto = new Gasto
                    {
                        Fecha = fecha,
                        CategoriaId = categoriaId.Value,
                        Tipo = "Variable",
                        Descripcion = parsed.Descripcion,
                        Monto = monto,
                        CuentaId = request.CuentaId,
                        UserId = userId
                    };
                    _context.Gastos.Add(gasto);
                    cuenta.BalanceActual -= monto;
                }
                else
                {
                    var ingreso = new Ingreso
                    {
                        Fecha = fecha,
                        CategoriaId = categoriaId.Value,
                        Descripcion = parsed.Descripcion,
                        Monto = monto,
                        CuentaId = request.CuentaId,
                        UserId = userId
                    };
                    _context.Ingresos.Add(ingreso);
                    cuenta.BalanceActual += monto;
                }

                result.FilasImportadas++;
            }

            importacion.FilasImportadas = result.FilasImportadas;
            importacion.FilasDuplicadas = result.FilasDuplicadas;
            importacion.FilasError = result.FilasError;
            importacion.Estado = result.FilasError > 0 && result.FilasImportadas > 0 ? "Parcial"
                               : result.FilasImportadas == 0 ? "Error" : "Completada";

            _context.ImportacionesCsv.Add(importacion);
            await _context.SaveChangesAsync();

            result.ImportacionId = importacion.Id;
            return result;
        }

        private async Task<CsvPreviewRowDto> ParseRowAsync(string userId, string[] row, int fila, CsvColumnMappingDto mapeo, CsvImportRequestDto request)
        {
            var preview = new CsvPreviewRowDto { Fila = fila };

            // Parse fecha
            if (mapeo.ColumnaFecha >= row.Length)
            {
                preview.Error = "Columna de fecha fuera de rango.";
                return preview;
            }

            if (!DateTime.TryParseExact(row[mapeo.ColumnaFecha].Trim(), mapeo.FormatoFecha,
                CultureInfo.InvariantCulture, DateTimeStyles.None, out var fecha))
            {
                // Intentar parse genérico
                if (!DateTime.TryParse(row[mapeo.ColumnaFecha].Trim(), CultureInfo.InvariantCulture, DateTimeStyles.None, out fecha))
                {
                    preview.Error = $"No se pudo parsear la fecha: '{row[mapeo.ColumnaFecha]}'";
                    return preview;
                }
            }
            preview.Fecha = fecha;

            // Parse monto
            if (mapeo.ColumnaMonto >= row.Length)
            {
                preview.Error = "Columna de monto fuera de rango.";
                return preview;
            }

            var montoStr = row[mapeo.ColumnaMonto].Trim().Replace("$", "").Replace(",", "");
            if (!decimal.TryParse(montoStr, NumberStyles.Any, CultureInfo.InvariantCulture, out var monto))
            {
                preview.Error = $"No se pudo parsear el monto: '{row[mapeo.ColumnaMonto]}'";
                return preview;
            }
            preview.Monto = monto;

            // Determinar tipo
            if (mapeo.MontoNegativoEsGasto)
            {
                preview.TipoDetectado = monto < 0 ? "Gasto" : "Ingreso";
            }
            else
            {
                preview.TipoDetectado = monto > 0 ? "Gasto" : "Ingreso";
            }

            // Descripción
            if (mapeo.ColumnaDescripcion.HasValue && mapeo.ColumnaDescripcion.Value < row.Length)
            {
                preview.Descripcion = row[mapeo.ColumnaDescripcion.Value].Trim();
            }

            // Sugerir categoría
            if (!string.IsNullOrEmpty(preview.Descripcion))
            {
                var sugerencia = await _reglasService.SugerirCategoriaAsync(userId, preview.Descripcion, preview.TipoDetectado);
                if (sugerencia != null)
                {
                    preview.CategoriaIdSugerida = sugerencia.CategoriaId;
                    preview.CategoriaNombreSugerida = sugerencia.CategoriaNombre;
                }
            }

            // Detectar duplicado
            var fechaDate = fecha.Date;
            var montoAbs = Math.Abs(monto);
            preview.EsDuplicado = await _context.Gastos.AnyAsync(g =>
                g.UserId == userId && g.CuentaId == request.CuentaId &&
                g.Fecha.Date == fechaDate && g.Monto == montoAbs &&
                g.Descripcion == preview.Descripcion)
            || await _context.Ingresos.AnyAsync(i =>
                i.UserId == userId && i.CuentaId == request.CuentaId &&
                i.Fecha.Date == fechaDate && i.Monto == montoAbs &&
                i.Descripcion == preview.Descripcion);

            return preview;
        }

        private static async Task<List<string[]>> ReadAllRowsAsync(Stream csvStream)
        {
            var rows = new List<string[]>();
            using var reader = new StreamReader(csvStream);
            var config = new CsvConfiguration(CultureInfo.InvariantCulture)
            {
                HasHeaderRecord = false,
                MissingFieldFound = null,
                BadDataFound = null
            };
            using var csv = new CsvReader(reader, config);

            while (await csv.ReadAsync())
            {
                var record = new List<string>();
                var index = 0;
                while (csv.TryGetField<string>(index, out var field))
                {
                    record.Add(field ?? "");
                    index++;
                }
                if (record.Count > 0)
                    rows.Add(record.ToArray());
            }

            return rows;
        }
    }
}
