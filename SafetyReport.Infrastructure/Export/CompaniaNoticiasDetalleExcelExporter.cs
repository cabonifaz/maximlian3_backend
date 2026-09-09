using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Spreadsheet;
using SafetyReport.Application.Ports.Compania;
using SafetyReport.Models;

namespace SafetyReport.Infrastructure.Export;

public class CompaniaNoticiasDetalleExcelExporter : ICompaniaNoticiasDetalleExcelExporter
{
    public byte[] GenerarExcelNoticiasDetalle(List<CompaniaNoticiaDetalleListaConsulta> items)
    {
        using var stream = new MemoryStream();
        using (var document = SpreadsheetDocument.Create(stream, SpreadsheetDocumentType.Workbook))
        {
            var workbookPart = document.AddWorkbookPart();
            workbookPart.Workbook = new Workbook();

            var worksheetPart = workbookPart.AddNewPart<WorksheetPart>();
            var sheetData = new SheetData();
            worksheetPart.Worksheet = new Worksheet();

            worksheetPart.Worksheet.Append(CrearColumnasExcel());
            worksheetPart.Worksheet.Append(sheetData);

            var sheets = workbookPart.Workbook.AppendChild(new Sheets());
            sheets.Append(new Sheet
            {
                Id = workbookPart.GetIdOfPart(worksheetPart),
                SheetId = 1,
                Name = "Detalle"
            });

            sheetData.Append(CrearFilaExcel(
                "IdCompania",
                "Nombre Completo",
                "Numero Documento",
                "Pais",
                "Bandera",
                "Direccion",
                "Telefono",
                "Actividad Comercial"
                ));

            foreach (var item in items)
            {
                sheetData.Append(CrearFilaExcel(
                    item.IdCompania.ToString(),
                    item.NombreCompleto,
                    item.NumeroDocumento,
                    item.Pais,
                    item.Bandera,
                    item.Direccion,
                    item.Telefono,
                    item.ActividadComercial
                    ));
            }

            workbookPart.Workbook.Save();
        }

        return stream.ToArray();
    }

    private static Columns CrearColumnasExcel()
    {
        return new Columns(
            CrearColumnaExcel(1, 1, 12),
            CrearColumnaExcel(2, 2, 35),
            CrearColumnaExcel(3, 3, 22),
            CrearColumnaExcel(4, 4, 20),
            CrearColumnaExcel(5, 5, 14),
            CrearColumnaExcel(6, 6, 45),
            CrearColumnaExcel(7, 7, 18),
            CrearColumnaExcel(8, 8, 30)
            );
    }

    private static Column CrearColumnaExcel(uint min, uint max, double width)
    {
        return new Column
        {
            Min = min,
            Max = max,
            Width = width,
            CustomWidth = true
        };
    }

    private static Row CrearFilaExcel(params string?[] valores)
    {
        var row = new Row();
        foreach (var valor in valores)
            row.Append(CrearCeldaExcel(valor));
        return row;
    }

    private static Cell CrearCeldaExcel(string? valor)
    {
        return new Cell
        {
            DataType = CellValues.InlineString,
            InlineString = new InlineString(new Text(valor ?? string.Empty))
        };
    }
}
