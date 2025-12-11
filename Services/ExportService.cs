using FinanzasPersonales.Api.Data;
using Microsoft.EntityFrameworkCore;
using OfficeOpenXml;
using OfficeOpenXml.Style;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using System.Text.Json;


namespace FinanzasPersonales.Api.Services
{
    /// <summary>
    /// Implementación del servicio de exportación de datos.
    /// </summary>
    public class ExportService : IExportService
    {
        private readonly FinanzasDbContext _context;

        public ExportService(FinanzasDbContext context)
        {
            _context = context;
            // Configurar licencia de EPPlus (NonCommercial o Commercial)
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;

            // Configurar licencia de QuestPDF
            QuestPDF.Settings.License = LicenseType.Community;
        }

        public async Task<byte[]> ExportToExcelAsync(string userId, DateTime desde, DateTime hasta, List<string> incluir)
        {
            using var package = new ExcelPackage();

            // Exportar Gastos
            if (incluir.Contains("gastos"))
            {
                var gastos = await _context.Gastos
                    .Include(g => g.Categoria)
                    .Where(g => g.UserId == userId && g.Fecha >= desde && g.Fecha <= hasta)
                    .OrderBy(g => g.Fecha)
                    .ToListAsync();

                var worksheetGastos = package.Workbook.Worksheets.Add("Gastos");

                // Encabezados
                worksheetGastos.Cells[1, 1].Value = "Fecha";
                worksheetGastos.Cells[1, 2].Value = "Categoría";
                worksheetGastos.Cells[1, 3].Value = "Tipo";
                worksheetGastos.Cells[1, 4].Value = "Descripción";
                worksheetGastos.Cells[1, 5].Value = "Monto";

                // Estilo encabezados
                using (var range = worksheetGastos.Cells[1, 1, 1, 5])
                {
                    range.Style.Font.Bold = true;
                    range.Style.Fill.PatternType = ExcelFillStyle.Solid;
                    range.Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.FromArgb(173, 216, 230));
                }

                // Datos
                int row = 2;
                foreach (var gasto in gastos)
                {
                    worksheetGastos.Cells[row, 1].Value = gasto.Fecha.ToString("yyyy-MM-dd");
                    worksheetGastos.Cells[row, 2].Value = gasto.Categoria?.Nombre ?? "N/A";
                    worksheetGastos.Cells[row, 3].Value = gasto.Tipo;
                    worksheetGastos.Cells[row, 4].Value = gasto.Descripcion ?? "";
                    worksheetGastos.Cells[row, 5].Value = gasto.Monto;
                    worksheetGastos.Cells[row, 5].Style.Numberformat.Format = "$#,##0.00";
                    row++;
                }

                worksheetGastos.Cells.AutoFitColumns();
            }

            // Exportar Ingresos
            if (incluir.Contains("ingresos"))
            {
                var ingresos = await _context.Ingresos
                    .Include(i => i.Categoria)
                    .Where(i => i.UserId == userId && i.Fecha >= desde && i.Fecha <= hasta)
                    .OrderBy(i => i.Fecha)
                    .ToListAsync();

                var worksheetIngresos = package.Workbook.Worksheets.Add("Ingresos");

                // Encabezados
                worksheetIngresos.Cells[1, 1].Value = "Fecha";
                worksheetIngresos.Cells[1, 2].Value = "Categoría";
                worksheetIngresos.Cells[1, 3].Value = "Monto";

                // Estilo encabezados
                using (var range = worksheetIngresos.Cells[1, 1, 1, 3])
                {
                    range.Style.Font.Bold = true;
                    range.Style.Fill.PatternType = ExcelFillStyle.Solid;
                    range.Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.FromArgb(144, 238, 144));
                }

                // Datos
                int row = 2;
                foreach (var ingreso in ingresos)
                {
                    worksheetIngresos.Cells[row, 1].Value = ingreso.Fecha.ToString("yyyy-MM-dd");
                    worksheetIngresos.Cells[row, 2].Value = ingreso.Categoria?.Nombre ?? "N/A";
                    worksheetIngresos.Cells[row, 3].Value = ingreso.Monto;
                    worksheetIngresos.Cells[row, 3].Style.Numberformat.Format = "$#,##0.00";
                    row++;
                }

                worksheetIngresos.Cells.AutoFitColumns();
            }

            // Exportar Presupuestos
            if (incluir.Contains("presupuestos"))
            {
                var presupuestos = await _context.Presupuestos
                    .Include(p => p.Categoria)
                    .Where(p => p.UserId == userId)
                    .ToListAsync();

                var worksheetPresupuestos = package.Workbook.Worksheets.Add("Presupuestos");

                // Encabezados
                worksheetPresupuestos.Cells[1, 1].Value = "Categoría";
                worksheetPresupuestos.Cells[1, 2].Value = "Límite";
                worksheetPresupuestos.Cells[1, 3].Value = "Período";
                worksheetPresupuestos.Cells[1, 4].Value = "Mes";
                worksheetPresupuestos.Cells[1, 5].Value = "Año";

                // Estilo encabezados
                using (var range = worksheetPresupuestos.Cells[1, 1, 1, 5])
                {
                    range.Style.Font.Bold = true;
                    range.Style.Fill.PatternType = ExcelFillStyle.Solid;
                    range.Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.FromArgb(255, 255, 224));
                }

                // Datos
                int row = 2;
                foreach (var presupuesto in presupuestos)
                {
                    worksheetPresupuestos.Cells[row, 1].Value = presupuesto.Categoria?.Nombre ?? "N/A";
                    worksheetPresupuestos.Cells[row, 2].Value = presupuesto.MontoLimite;
                    worksheetPresupuestos.Cells[row, 2].Style.Numberformat.Format = "$#,##0.00";
                    worksheetPresupuestos.Cells[row, 3].Value = presupuesto.Periodo;
                    worksheetPresupuestos.Cells[row, 4].Value = presupuesto.MesAplicable;
                    worksheetPresupuestos.Cells[row, 5].Value = presupuesto.AnoAplicable;
                    row++;
                }

                worksheetPresupuestos.Cells.AutoFitColumns();
            }

            // Exportar Metas
            if (incluir.Contains("metas"))
            {
                var metas = await _context.Metas
                    .Where(m => m.UserId == userId)
                    .ToListAsync();

                var worksheetMetas = package.Workbook.Worksheets.Add("Metas");

                // Encabezados
                worksheetMetas.Cells[1, 1].Value = "Meta";
                worksheetMetas.Cells[1, 2].Value = "Monto Total";
                worksheetMetas.Cells[1, 3].Value = "Ahorro Actual";
                worksheetMetas.Cells[1, 4].Value = "Monto Restante";

                // Estilo encabezados
                using (var range = worksheetMetas.Cells[1, 1, 1, 4])
                {
                    range.Style.Font.Bold = true;
                    range.Style.Fill.PatternType = ExcelFillStyle.Solid;
                    range.Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.FromArgb(240, 128, 128));
                }

                // Datos
                int row = 2;
                foreach (var meta in metas)
                {
                    worksheetMetas.Cells[row, 1].Value = meta.Metas;
                    worksheetMetas.Cells[row, 2].Value = meta.MontoTotal;
                    worksheetMetas.Cells[row, 2].Style.Numberformat.Format = "$#,##0.00";
                    worksheetMetas.Cells[row, 3].Value = meta.AhorroActual;
                    worksheetMetas.Cells[row, 3].Style.Numberformat.Format = "$#,##0.00";
                    worksheetMetas.Cells[row, 4].Value = meta.MontoRestante;
                    worksheetMetas.Cells[row, 4].Style.Numberformat.Format = "$#,##0.00";
                    row++;
                }

                worksheetMetas.Cells.AutoFitColumns();
            }

            return package.GetAsByteArray();
        }

        public async Task<byte[]> ExportToPdfAsync(string userId, DateTime desde, DateTime hasta, List<string> incluir)
        {
            var document = Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A4);
                    page.Margin(2, Unit.Centimetre);
                    page.PageColor(Colors.White);
                    page.DefaultTextStyle(x => x.FontSize(10));

                    page.Header()
                        .Text($"Reporte Financiero - {DateTime.Now:yyyy-MM-dd}")
                        .SemiBold().FontSize(16).FontColor(Colors.Blue.Medium);

                    page.Content()
                        .PaddingVertical(1, Unit.Centimetre)
                        .Column(col =>
                        {
                            col.Spacing(20);

                            // Período
                            col.Item().Text($"Período: {desde:yyyy-MM-dd} a {hasta:yyyy-MM-dd}").FontSize(12);

                            // Gastos
                            if (incluir.Contains("gastos"))
                            {
                                var gastos = _context.Gastos
                                    .Include(g => g.Categoria)
                                    .Where(g => g.UserId == userId && g.Fecha >= desde && g.Fecha <= hasta)
                                    .OrderBy(g => g.Fecha)
                                    .ToList();

                                col.Item().Text("Gastos").Bold().FontSize(14);
                                col.Item().Table(table =>
                                {
                                    table.ColumnsDefinition(columns =>
                                    {
                                        columns.ConstantColumn(80);
                                        columns.RelativeColumn();
                                        columns.ConstantColumn(60);
                                        columns.ConstantColumn(80);
                                    });

                                    // Encabezados
                                    table.Header(header =>
                                    {
                                        header.Cell().Background(Colors.Grey.Lighten3).Padding(5).Text("Fecha");
                                        header.Cell().Background(Colors.Grey.Lighten3).Padding(5).Text("Categoría");
                                        header.Cell().Background(Colors.Grey.Lighten3).Padding(5).Text("Tipo");
                                        header.Cell().Background(Colors.Grey.Lighten3).Padding(5).Text("Monto");
                                    });

                                    foreach (var gasto in gastos)
                                    {
                                        table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(5).Text(gasto.Fecha.ToString("yyyy-MM-dd"));
                                        table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(5).Text(gasto.Categoria?.Nombre ?? "N/A");
                                        table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(5).Text(gasto.Tipo);
                                        table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(5).Text($"${gasto.Monto:N2}");
                                    }
                                });
                            }

                            // Ingresos
                            if (incluir.Contains("ingresos"))
                            {
                                var ingresos = _context.Ingresos
                                    .Include(i => i.Categoria)
                                    .Where(i => i.UserId == userId && i.Fecha >= desde && i.Fecha <= hasta)
                                    .OrderBy(i => i.Fecha)
                                    .ToList();

                                col.Item().Text("Ingresos").Bold().FontSize(14);
                                col.Item().Table(table =>
                                {
                                    table.ColumnsDefinition(columns =>
                                    {
                                        columns.ConstantColumn(100);
                                        columns.RelativeColumn();
                                        columns.ConstantColumn(100);
                                    });

                                    // Encabezados
                                    table.Header(header =>
                                    {
                                        header.Cell().Background(Colors.Grey.Lighten3).Padding(5).Text("Fecha");
                                        header.Cell().Background(Colors.Grey.Lighten3).Padding(5).Text("Categoría");
                                        header.Cell().Background(Colors.Grey.Lighten3).Padding(5).Text("Monto");
                                    });

                                    foreach (var ingreso in ingresos)
                                    {
                                        table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(5).Text(ingreso.Fecha.ToString("yyyy-MM-dd"));
                                        table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(5).Text(ingreso.Categoria?.Nombre ?? "N/A");
                                        table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(5).Text($"${ingreso.Monto:N2}");
                                    }
                                });
                            }
                        });

                    page.Footer()
                        .AlignCenter()
                        .Text(x =>
                        {
                            x.Span("Página ");
                            x.CurrentPageNumber();
                        });
                });
            });

            return document.GeneratePdf();
        }

        public async Task<string> ExportToJsonAsync(string userId, DateTime? desde = null, DateTime? hasta = null)
        {
            var backup = new
            {
                Categorias = await _context.Categorias
                    .Where(c => c.UserId == userId)
                    .Select(c => new { c.Id, c.Nombre, c.Tipo })
                    .ToListAsync(),

                Gastos = await _context.Gastos
                    .Where(g => g.UserId == userId &&
                               (!desde.HasValue || g.Fecha >= desde.Value) &&
                               (!hasta.HasValue || g.Fecha <= hasta.Value))
                    .Select(g => new { g.Id, g.Fecha, g.CategoriaId, g.Tipo, g.Descripcion, g.Monto })
                    .ToListAsync(),

                Ingresos = await _context.Ingresos
                    .Where(i => i.UserId == userId &&
                               (!desde.HasValue || i.Fecha >= desde.Value) &&
                               (!hasta.HasValue || i.Fecha <= hasta.Value))
                    .Select(i => new { i.Id, i.Fecha, i.CategoriaId, i.Monto })
                    .ToListAsync(),

                Metas = await _context.Metas
                    .Where(m => m.UserId == userId)
                    .Select(m => new { m.Id, m.Metas, m.MontoTotal, m.AhorroActual, m.MontoRestante })
                    .ToListAsync(),

                Presupuestos = await _context.Presupuestos
                    .Where(p => p.UserId == userId)
                    .Select(p => new { p.Id, p.CategoriaId, p.MontoLimite, p.Periodo, p.MesAplicable, p.AnoAplicable })
                    .ToListAsync(),

                ExportDate = DateTime.Now,
                Version = "1.0"
            };

            return JsonSerializer.Serialize(backup, new JsonSerializerOptions
            {
                WriteIndented = true
            });
        }
    }
}
