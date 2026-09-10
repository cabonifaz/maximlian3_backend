using System.Globalization;
using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Spreadsheet;
using SafetyReport.Application.Puertos.PedidoFactura;

namespace SafetyReport.Infrastructure.Exportacion;

public class PedidoPrefacturaExcelExporter : IPedidoPrefacturaExcelExporter
{
    private const int IndiceNr = 0;
    private const int IndiceCompany = 2;
    private const int IndicePrecio = 5;

    private static readonly double[] AnchosColumna = { 6, 26, 70, 18, 18, 20, 15, 15, 45, 60 };

    public byte[] GenerarExcelPrefactura(List<string> headers, List<PedidoPrefacturaConsulta> items)
    {
        using var stream = new MemoryStream();
        using (var document = SpreadsheetDocument.Create(stream, SpreadsheetDocumentType.Workbook))
        {
            var workbookPart = document.AddWorkbookPart();
            workbookPart.Workbook = new Workbook();
            workbookPart.AddNewPart<WorkbookStylesPart>().Stylesheet = CrearStylesheetPrefactura();

            var worksheetPart = workbookPart.AddNewPart<WorksheetPart>();
            var sheetData = new SheetData();
            worksheetPart.Worksheet = new Worksheet();
            worksheetPart.Worksheet.Append(sheetData);

            var sheets = workbookPart.Workbook.AppendChild(new Sheets());
            sheets.Append(new Sheet
            {
                Id = workbookPart.GetIdOfPart(worksheetPart),
                SheetId = 1,
                Name = "Reports"
            });

            sheetData.Append(CrearFilaEncabezadoExcel(headers));

            var filaValores = new List<object?>[items.Count];
            for (var i = 0; i < items.Count; i++)
            {
                var it = items[i];
                filaValores[i] = new List<object?>
                {
                    it.Nr, it.ReferenceNumber, it.Company, it.DateOfRequest, it.DeliveryDate,
                    it.Amount, it.Currency, it.Status, it.Country, it.Observation
                };
            }

            foreach (var valores in filaValores)
                sheetData.Append(CrearFilaDatosExcel(valores));

            if (headers.Count > 0)
                worksheetPart.Worksheet.InsertBefore(CrearColumnasAnchoAjustado(headers), sheetData);

            if (headers.Count > 0)
            {
                var ultimaColumna = ObtenerLetraColumna(headers.Count - 1);
                var ultimaFila = items.Count + 1;
                worksheetPart.Worksheet.InsertAfter(
                    new AutoFilter { Reference = $"A1:{ultimaColumna}{ultimaFila}" }, sheetData);
            }

            workbookPart.Workbook.Save();
        }

        return stream.ToArray();
    }

    private static Row CrearFilaEncabezadoExcel(List<string> headers)
    {
        var row = new Row();
        foreach (var texto in headers)
            row.Append(new Cell
            {
                StyleIndex = 1,
                DataType = CellValues.InlineString,
                InlineString = new InlineString(new Text(texto))
            });
        return row;
    }

    private static Row CrearFilaDatosExcel(List<object?> valores)
    {
        var row = new Row();
        for (var i = 0; i < valores.Count; i++)
        {
            if (i == IndicePrecio && valores[i] is decimal precio)
            {
                row.Append(new Cell
                {
                    StyleIndex = 3,
                    DataType = CellValues.Number,
                    CellValue = new CellValue(precio.ToString(CultureInfo.InvariantCulture))
                });
                continue;
            }

            if (i == IndiceNr && valores[i] is int nr)
            {
                row.Append(new Cell
                {
                    StyleIndex = 4,
                    DataType = CellValues.Number,
                    CellValue = new CellValue(nr.ToString(CultureInfo.InvariantCulture))
                });
                continue;
            }

            if (i == IndiceCompany)
            {
                row.Append(new Cell
                {
                    StyleIndex = 5,
                    DataType = CellValues.InlineString,
                    InlineString = new InlineString(new Text(valores[i]?.ToString() ?? string.Empty))
                });
                continue;
            }

            row.Append(new Cell
            {
                StyleIndex = 2,
                DataType = CellValues.InlineString,
                InlineString = new InlineString(new Text(valores[i]?.ToString() ?? string.Empty))
            });
        }
        return row;
    }

    private static string ObtenerLetraColumna(int indiceCero)
    {
        var letras = string.Empty;
        var n = indiceCero;
        do
        {
            letras = (char)('A' + n % 26) + letras;
            n = n / 26 - 1;
        } while (n >= 0);
        return letras;
    }

    private static Columns CrearColumnasAnchoAjustado(List<string> headers)
    {
        var columnas = new Columns();
        for (var i = 0; i < headers.Count; i++)
        {
            var ancho = i < AnchosColumna.Length ? AnchosColumna[i] : 20;
            var indiceColumna = (uint)(i + 1);
            columnas.Append(new Column
            {
                Min = indiceColumna,
                Max = indiceColumna,
                Width = ancho,
                CustomWidth = true
            });
        }

        return columnas;
    }

    private static Stylesheet CrearStylesheetPrefactura()
    {
        var fuentes = new Fonts(
            new Font(new FontSize { Val = 11 }, new FontName { Val = "Calibri" }),
            new Font(new Bold(), new Color { Rgb = "FFFFFFFF" }, new FontSize { Val = 11 }, new FontName { Val = "Calibri" }),
            new Font(new Bold(), new FontSize { Val = 11 }, new FontName { Val = "Calibri" }));

        var rellenos = new Fills(
            new Fill(new PatternFill { PatternType = PatternValues.None }),
            new Fill(new PatternFill { PatternType = PatternValues.Gray125 }),
            new Fill(new PatternFill(new ForegroundColor { Rgb = "FF790303" }) { PatternType = PatternValues.Solid }));

        var bordeFino = new Border(
            new LeftBorder(new Color { Rgb = "FFBFBFBF" }) { Style = BorderStyleValues.Thin },
            new RightBorder(new Color { Rgb = "FFBFBFBF" }) { Style = BorderStyleValues.Thin },
            new TopBorder(new Color { Rgb = "FFBFBFBF" }) { Style = BorderStyleValues.Thin },
            new BottomBorder(new Color { Rgb = "FFBFBFBF" }) { Style = BorderStyleValues.Thin },
            new DiagonalBorder());

        var bordes = new Borders(new Border(new LeftBorder(), new RightBorder(), new TopBorder(), new BottomBorder(), new DiagonalBorder()), bordeFino);

        static Alignment Centrado() => new() { Horizontal = HorizontalAlignmentValues.Center };
        static Alignment Izquierda() => new() { Horizontal = HorizontalAlignmentValues.Left };

        var formatosCelda = new CellFormats(
            new CellFormat(),
            new CellFormat { FontId = 1, FillId = 2, BorderId = 1, Alignment = Centrado(), ApplyFont = true, ApplyFill = true, ApplyBorder = true, ApplyAlignment = true },
            new CellFormat { FontId = 0, BorderId = 1, Alignment = Centrado(), ApplyBorder = true, ApplyAlignment = true },
            new CellFormat { FontId = 0, BorderId = 1, NumberFormatId = 4, Alignment = Centrado(), ApplyBorder = true, ApplyNumberFormat = true, ApplyAlignment = true },
            new CellFormat { FontId = 2, BorderId = 1, Alignment = Centrado(), ApplyFont = true, ApplyBorder = true, ApplyAlignment = true },
            new CellFormat { FontId = 0, BorderId = 1, Alignment = Izquierda(), ApplyBorder = true, ApplyAlignment = true });

        return new Stylesheet(fuentes, rellenos, bordes, formatosCelda);
    }
}
