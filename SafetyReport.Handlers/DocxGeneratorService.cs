using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.RegularExpressions;
using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Wordprocessing;

namespace SafetyReport.Handlers;

public class DocxGeneratorService
{
    public MemoryStream GenerarDocx(JsonNode json)
    {
        var ms = new MemoryStream();
        using (var doc = WordprocessingDocument.Create(ms, WordprocessingDocumentType.Document, true))
        {
            var mainPart = doc.AddMainDocumentPart();
            mainPart.Document = new Document();
            var body = new Body();

            var config = json["document"];
            var sections = json["sections"]?.AsArray();

            if (sections != null)
            {
                foreach (var section in sections)
                {
                    if (section is null) continue;
                    RenderizarSeccion(body, section, config);
                }
            }

            AgregarPropiedadesPagina(body, config);
            AgregarPiePagina(mainPart, config);

            mainPart.Document.Append(body);
            mainPart.Document.Save();
        }

        ms.Position = 0;
        return ms;
    }

    private void RenderizarSeccion(Body body, JsonNode section, JsonNode? config)
    {
        var type = section["type"]?.GetValue<string>() ?? "";

        switch (type)
        {
            case "heading":
                RenderizarHeading(body, section);
                break;
            case "subtitle":
                RenderizarSubtitle(body, section);
                break;
            case "text":
                RenderizarText(body, section);
                break;
            case "keyValue":
                RenderizarKeyValue(body, section);
                break;
            case "borderedBox":
                RenderizarBorderedBox(body, section);
                break;
            case "referenceBox":
                RenderizarReferenceBox(body, section);
                break;
            case "dataTable":
                RenderizarDataTable(body, section);
                break;
            case "repeat":
                var subs = section["sections"]?.AsArray();
                if (subs != null)
                    foreach (var sub in subs)
                        if (sub != null) RenderizarSeccion(body, sub, config);
                break;
            case "repeatDetail":
                RenderizarRepeatDetail(body, section);
                break;
            case "spacer":
                RenderizarSpacer(body, section);
                break;
        }
    }

    private void RenderizarHeading(Body body, JsonNode section)
    {
        var text = section["text"]?.GetValue<string>() ?? "";
        var style = ParseCss(section["style"]?.GetValue<string>());
        var level = section["level"]?.GetValue<int>() ?? 2;

        var para = new Paragraph();
        var pPr = new ParagraphProperties();

        if (style.TryGetValue("text-align", out var align))
            pPr.Append(new Justification { Val = MapearAlineacion(align) });

        AgregarEspaciado(pPr, style);

        para.Append(pPr);

        var run = new Run();
        var rPr = new RunProperties();

        var fontSize = level == 1 ? "24" : "20";
        if (style.TryGetValue("font-size", out var fs))
            fontSize = ConvertirPtAHalfPt(fs);

        rPr.Append(new FontSize { Val = fontSize });
        rPr.Append(new RunFonts { Ascii = "Calibri", HighAnsi = "Calibri" });

        if (style.GetValueOrDefault("font-weight") == "700" || style.GetValueOrDefault("font-weight") == "bold")
            rPr.Append(new Bold());

        run.Append(rPr);
        run.Append(new Text(text) { Space = SpaceProcessingModeValues.Preserve });
        para.Append(run);
        body.Append(para);
    }

    private void RenderizarSubtitle(Body body, JsonNode section)
    {
        var text = section["text"]?.GetValue<string>() ?? "";
        var style = ParseCss(section["style"]?.GetValue<string>());

        var para = new Paragraph();
        var pPr = new ParagraphProperties();
        AgregarEspaciado(pPr, style);
        para.Append(pPr);

        var run = new Run();
        var rPr = new RunProperties();
        rPr.Append(new FontSize { Val = "20" });
        rPr.Append(new RunFonts { Ascii = "Calibri", HighAnsi = "Calibri" });

        if (style.GetValueOrDefault("font-weight") == "700" || style.GetValueOrDefault("font-weight") == "bold")
            rPr.Append(new Bold());

        run.Append(rPr);
        run.Append(new Text(text) { Space = SpaceProcessingModeValues.Preserve });
        para.Append(run);
        body.Append(para);
    }

    private void RenderizarText(Body body, JsonNode section)
    {
        var text = section["field"]?.GetValue<string>() ?? "";
        if (string.IsNullOrEmpty(text)) return;

        var lines = text.Split('\n');
        foreach (var line in lines)
        {
            var para = new Paragraph();
            var pPr = new ParagraphProperties();
            pPr.Append(new SpacingBetweenLines { After = "0", Line = "276", LineRule = LineSpacingRuleValues.Auto });
            para.Append(pPr);

            var run = new Run();
            var rPr = new RunProperties();
            rPr.Append(new FontSize { Val = "20" });
            rPr.Append(new RunFonts { Ascii = "Calibri", HighAnsi = "Calibri" });
            run.Append(rPr);
            run.Append(new Text(line) { Space = SpaceProcessingModeValues.Preserve });
            para.Append(run);
            body.Append(para);
        }
    }

    private void RenderizarKeyValue(Body body, JsonNode section)
    {
        var style = ParseCss(section["style"]?.GetValue<string>());
        var lblStyle = ParseCss(section["labelStyle"]?.GetValue<string>());
        var rows = section["rows"]?.AsArray();
        if (rows is null || rows.Count == 0) return;

        var table = new Table();
        var tPr = CrearPropiedadesTabla(style);
        table.Append(tPr);

        var lblWidth = ExtraerAncho(lblStyle);

        foreach (var row in rows)
        {
            if (row is null) continue;
            var label = row["label"]?.GetValue<string>() ?? "";
            var value = row["value"]?.GetValue<string>() ?? "";
            var separator = row["separator"]?.GetValue<string>() ?? "";

            var tr = new TableRow();

            var tcLabel = CrearCelda(label, lblWidth, true, lblStyle);
            tr.Append(tcLabel);

            var tcValue = CrearCelda(separator + value, null, false, null);
            tr.Append(tcValue);

            table.Append(tr);
        }

        body.Append(table);
        body.Append(new Paragraph(new ParagraphProperties(new SpacingBetweenLines { After = "0" })));
    }

    private void RenderizarBorderedBox(Body body, JsonNode section)
    {
        var style = ParseCss(section["style"]?.GetValue<string>());
        var lblStyle = ParseCss(section["labelStyle"]?.GetValue<string>());
        var valStyle = ParseCss(section["valueStyle"]?.GetValue<string>());
        var title = section["title"]?.GetValue<string>() ?? "";
        var content = section["content"]?.GetValue<string>();
        var rows = section["rows"]?.AsArray();

        var table = new Table();
        var tPr = CrearPropiedadesTabla(style, true);
        table.Append(tPr);

        var lblWidth = ExtraerAncho(lblStyle);

        // Title row
        var trTitle = new TableRow();
        var tcTitle = CrearCelda(title, null, true, ParseCss(section["titleStyle"]?.GetValue<string>()), 2);
        trTitle.Append(tcTitle);
        table.Append(trTitle);

        // Content row
        if (!string.IsNullOrEmpty(content))
        {
            var trContent = new TableRow();
            var tcContent = CrearCelda(content, null, false, null, 2);
            trContent.Append(tcContent);
            table.Append(trContent);
        }

        // Data rows
        if (rows != null)
        {
            foreach (var row in rows)
            {
                if (row is null) continue;
                var label = row["label"]?.GetValue<string>() ?? "";
                var value = row["value"]?.GetValue<string>() ?? "";
                var rowStyle = ParseCss(row["style"]?.GetValue<string>());
                var isBold = rowStyle.GetValueOrDefault("font-weight") == "700" ||
                             lblStyle.GetValueOrDefault("font-weight") == "700";

                var tr = new TableRow();
                tr.Append(CrearCelda(label, lblWidth, isBold, lblStyle.Count > 0 ? lblStyle : null, borders: true));
                tr.Append(CrearCelda(value, null, rowStyle.GetValueOrDefault("font-weight") == "700", valStyle.Count > 0 ? valStyle : null, borders: true));
                table.Append(tr);
            }
        }

        body.Append(table);
        body.Append(new Paragraph(new ParagraphProperties(new SpacingBetweenLines { After = "0" })));
    }

    private void RenderizarReferenceBox(Body body, JsonNode section)
    {
        var title = section["title"]?.GetValue<string>() ?? "";
        var items = section["items"]?.AsArray();

        var table = new Table();
        var tPr = CrearPropiedadesTabla(ParseCss(section["style"]?.GetValue<string>()), true);
        table.Append(tPr);

        var trTitle = new TableRow();
        trTitle.Append(CrearCelda(title, null, true, ParseCss(section["titleStyle"]?.GetValue<string>())));
        table.Append(trTitle);

        if (items != null)
        {
            foreach (var item in items)
            {
                var text = item?.GetValue<string>() ?? "";
                var tr = new TableRow();
                tr.Append(CrearCelda(text, null, false, null, fontSize: "12"));
                table.Append(tr);
            }
        }

        body.Append(table);
        body.Append(new Paragraph(new ParagraphProperties(new SpacingBetweenLines { After = "0" })));
    }

    private void RenderizarDataTable(Body body, JsonNode section)
    {
        var style = ParseCss(section["style"]?.GetValue<string>());
        var columns = section["columns"]?.AsArray();
        var rows = section["rows"]?.AsArray();
        var columnWidths = section["columnWidths"]?.AsArray();
        var headerStyle = ParseCss(section["headerStyle"]?.GetValue<string>());

        if (columns is null) return;

        var table = new Table();
        var tPr = CrearPropiedadesTabla(style);
        table.Append(tPr);

        // Header row
        var trHeader = new TableRow();
        for (int i = 0; i < columns.Count; i++)
        {
            var col = columns[i];
            var header = col?["header"]?.GetValue<string>() ?? "";
            var width = columnWidths != null && i < columnWidths.Count
                ? columnWidths[i]?.GetValue<string>()
                : null;
            var isBold = headerStyle.GetValueOrDefault("font-weight") == "bold" ||
                         headerStyle.GetValueOrDefault("font-weight") == "700";
            trHeader.Append(CrearCelda(header, width, isBold, null));
        }
        table.Append(trHeader);

        // Data rows
        if (rows != null)
        {
            foreach (var row in rows)
            {
                if (row is null) continue;
                var tr = new TableRow();

                if (row is JsonArray arr)
                {
                    for (int i = 0; i < arr.Count; i++)
                    {
                        var width = columnWidths != null && i < columnWidths.Count
                            ? columnWidths[i]?.GetValue<string>()
                            : null;
                        tr.Append(CrearCelda(arr[i]?.GetValue<string>() ?? "", width, false, null));
                    }
                }
                else if (row is JsonObject obj)
                {
                    int i = 0;
                    foreach (var prop in obj)
                    {
                        var width = columnWidths != null && i < columnWidths.Count
                            ? columnWidths[i]?.GetValue<string>()
                            : null;
                        tr.Append(CrearCelda(prop.Value?.GetValue<string>() ?? "", width, false, null));
                        i++;
                    }
                }

                table.Append(tr);
            }
        }

        body.Append(table);
        body.Append(new Paragraph(new ParagraphProperties(new SpacingBetweenLines { After = "0" })));
    }

    private void RenderizarRepeatDetail(Body body, JsonNode section)
    {
        var items = section["items"]?.AsArray();
        if (items is null) return;

        var titleStyle = ParseCss(section["titleStyle"]?.GetValue<string>());

        foreach (var item in items)
        {
            if (item is null) continue;
            var title = item["title"]?.GetValue<string>() ?? "";
            var content = item["content"]?.GetValue<string>() ?? "";

            // Title
            var pTitle = new Paragraph();
            var pPrTitle = new ParagraphProperties();
            AgregarEspaciado(pPrTitle, titleStyle);
            pTitle.Append(pPrTitle);

            var rTitle = new Run();
            var rPrTitle = new RunProperties();
            rPrTitle.Append(new FontSize { Val = "20" });
            rPrTitle.Append(new RunFonts { Ascii = "Calibri", HighAnsi = "Calibri" });
            if (titleStyle.GetValueOrDefault("font-weight") == "700")
                rPrTitle.Append(new Bold());
            rTitle.Append(rPrTitle);
            rTitle.Append(new Text(title) { Space = SpaceProcessingModeValues.Preserve });
            pTitle.Append(rTitle);
            body.Append(pTitle);

            // Content
            if (!string.IsNullOrEmpty(content))
            {
                foreach (var line in content.Split('\n'))
                {
                    var pContent = new Paragraph();
                    pContent.Append(new ParagraphProperties(new SpacingBetweenLines { After = "0", Line = "276", LineRule = LineSpacingRuleValues.Auto }));
                    var rContent = new Run();
                    var rPrContent = new RunProperties();
                    rPrContent.Append(new FontSize { Val = "20" });
                    rPrContent.Append(new RunFonts { Ascii = "Calibri", HighAnsi = "Calibri" });
                    rContent.Append(rPrContent);
                    rContent.Append(new Text(line) { Space = SpaceProcessingModeValues.Preserve });
                    pContent.Append(rContent);
                    body.Append(pContent);
                }
            }
        }
    }

    private void RenderizarSpacer(Body body, JsonNode section)
    {
        var height = section["height"]?.GetValue<string>() ?? "0.3in";
        var twips = ConvertirInchATwips(height);

        var para = new Paragraph();
        var pPr = new ParagraphProperties();
        pPr.Append(new SpacingBetweenLines { Before = twips.ToString(), After = "0" });
        para.Append(pPr);
        body.Append(para);
    }

    private void AgregarPropiedadesPagina(Body body, JsonNode? config)
    {
        if (config is null) return;

        var pageW = ConvertirInchATwips(config["pageSize"]?["width"]?.GetValue<string>() ?? "8.27in");
        var pageH = ConvertirInchATwips(config["pageSize"]?["height"]?.GetValue<string>() ?? "11.69in");
        var mt = ConvertirInchATwips(config["margins"]?["top"]?.GetValue<string>() ?? "1.15in");
        var mb = ConvertirInchATwips(config["margins"]?["bottom"]?.GetValue<string>() ?? "1.0in");
        var ml = ConvertirInchATwips(config["margins"]?["left"]?.GetValue<string>() ?? "0.5in");
        var mr = ConvertirInchATwips(config["margins"]?["right"]?.GetValue<string>() ?? "0.5in");

        var secPr = new SectionProperties();
        secPr.Append(new PageSize
        {
            Width = (UInt32Value)(uint)pageW,
            Height = (UInt32Value)(uint)pageH
        });
        secPr.Append(new PageMargin
        {
            Top = mt,
            Bottom = mb,
            Left = (UInt32Value)(uint)ml,
            Right = (UInt32Value)(uint)mr
        });

        body.Append(secPr);
    }

    private void AgregarPiePagina(MainDocumentPart mainPart, JsonNode? config)
    {
        var footerText = config?["footer"]?["text"]?.GetValue<string>();
        if (string.IsNullOrEmpty(footerText)) return;

        var footerPart = mainPart.AddNewPart<FooterPart>();
        var footer = new Footer();

        var para = new Paragraph();
        var rPr = new RunProperties();
        rPr.Append(new FontSize { Val = "14" });
        rPr.Append(new RunFonts { Ascii = "Calibri", HighAnsi = "Calibri" });

        var run = new Run();
        run.Append(rPr);
        run.Append(new Text(footerText) { Space = SpaceProcessingModeValues.Preserve });
        para.Append(run);
        footer.Append(para);

        // Page number
        var paraPage = new Paragraph();
        var runPage = new Run();
        runPage.Append(new RunProperties(new FontSize { Val = "14" }, new RunFonts { Ascii = "Calibri", HighAnsi = "Calibri" }));
        runPage.Append(new Text("Page ") { Space = SpaceProcessingModeValues.Preserve });
        runPage.Append(new FieldChar { FieldCharType = FieldCharValues.Begin });
        runPage.Append(new FieldCode(" PAGE ") { Space = SpaceProcessingModeValues.Preserve });
        runPage.Append(new FieldChar { FieldCharType = FieldCharValues.End });
        paraPage.Append(runPage);
        footer.Append(paraPage);

        footerPart.Footer = footer;
        footerPart.Footer.Save();

        var footerRef = new FooterReference
        {
            Type = HeaderFooterValues.Default,
            Id = mainPart.GetIdOfPart(footerPart)
        };

        var secPr = mainPart.Document.Body?.Elements<SectionProperties>().FirstOrDefault();
        secPr?.InsertAt(footerRef, 0);
    }

    // === Helpers ===

    private TableProperties CrearPropiedadesTabla(Dictionary<string, string> style, bool allBorders = false)
    {
        var tPr = new TableProperties();

        if (style.TryGetValue("width", out var w))
        {
            var twips = ConvertirInchATwips(w);
            tPr.Append(new TableWidth { Width = twips.ToString(), Type = TableWidthUnitValues.Dxa });
        }
        else
        {
            tPr.Append(new TableWidth { Width = "5000", Type = TableWidthUnitValues.Pct });
        }

        if (style.ContainsKey("margin"))
        {
            var margin = style["margin"];
            if (margin.Contains("auto"))
                tPr.Append(new TableJustification { Val = TableRowAlignmentValues.Center });
        }

        if (allBorders || style.ContainsKey("border"))
        {
            tPr.Append(new TableBorders(
                new TopBorder { Val = BorderValues.Single, Size = 4, Space = 0, Color = "000000" },
                new BottomBorder { Val = BorderValues.Single, Size = 4, Space = 0, Color = "000000" },
                new LeftBorder { Val = BorderValues.Single, Size = 4, Space = 0, Color = "000000" },
                new RightBorder { Val = BorderValues.Single, Size = 4, Space = 0, Color = "000000" },
                new InsideHorizontalBorder { Val = BorderValues.Single, Size = 4, Space = 0, Color = "000000" },
                new InsideVerticalBorder { Val = BorderValues.Single, Size = 4, Space = 0, Color = "000000" }
            ));
        }

        tPr.Append(new TableLayout { Type = TableLayoutValues.Fixed });

        return tPr;
    }

    private TableCell CrearCelda(string text, string? width, bool bold, Dictionary<string, string>? style,
        int colspan = 1, bool borders = false, string? fontSize = null)
    {
        var tc = new TableCell();
        var tcPr = new TableCellProperties();

        if (width != null)
        {
            var twips = ConvertirInchATwips(width);
            tcPr.Append(new TableCellWidth { Width = twips.ToString(), Type = TableWidthUnitValues.Dxa });
        }

        if (colspan > 1)
            tcPr.Append(new GridSpan { Val = colspan });

        if (borders && style != null && style.ContainsKey("border"))
        {
            tcPr.Append(new TableCellBorders(
                new TopBorder { Val = BorderValues.Single, Size = 4, Space = 0, Color = "000000" },
                new BottomBorder { Val = BorderValues.Single, Size = 4, Space = 0, Color = "000000" },
                new LeftBorder { Val = BorderValues.Single, Size = 4, Space = 0, Color = "000000" },
                new RightBorder { Val = BorderValues.Single, Size = 4, Space = 0, Color = "000000" }
            ));
        }

        tc.Append(tcPr);

        var para = new Paragraph();
        var pPr = new ParagraphProperties();
        pPr.Append(new SpacingBetweenLines { After = "0", Line = "240" });

        if (style != null && style.TryGetValue("text-align", out var align))
            pPr.Append(new Justification { Val = MapearAlineacion(align) });

        para.Append(pPr);

        var run = new Run();
        var rPr = new RunProperties();
        rPr.Append(new FontSize { Val = fontSize ?? "20" });
        rPr.Append(new RunFonts { Ascii = "Calibri", HighAnsi = "Calibri" });

        if (bold)
            rPr.Append(new Bold());

        run.Append(rPr);

        // Handle multi-line text
        var lines = text.Split('\n');
        for (int i = 0; i < lines.Length; i++)
        {
            if (i > 0)
                run.Append(new Break());
            run.Append(new Text(lines[i]) { Space = SpaceProcessingModeValues.Preserve });
        }

        para.Append(run);
        tc.Append(para);
        return tc;
    }

    private void AgregarEspaciado(ParagraphProperties pPr, Dictionary<string, string> style)
    {
        int before = 0, after = 0;

        if (style.TryGetValue("margin", out var margin))
        {
            var parts = margin.Split(' ');
            if (parts.Length >= 1) before = ConvertirPtATwips(parts[0]);
            if (parts.Length >= 3) after = ConvertirPtATwips(parts[2]);
            else if (parts.Length >= 2) after = ConvertirPtATwips(parts[0]);
        }

        if (style.TryGetValue("padding-top", out var pt))
            before = ConvertirPtATwips(pt);
        if (style.TryGetValue("padding-bottom", out var pb))
            after = ConvertirPtATwips(pb);
        if (style.TryGetValue("margin-top", out var mt))
            before = ConvertirPtATwips(mt);
        if (style.TryGetValue("margin-bottom", out var mb))
            after = ConvertirPtATwips(mb);

        pPr.Append(new SpacingBetweenLines
        {
            Before = before.ToString(),
            After = after.ToString(),
            Line = "276",
            LineRule = LineSpacingRuleValues.Auto
        });
    }

    private static Dictionary<string, string> ParseCss(string? css)
    {
        var result = new Dictionary<string, string>();
        if (string.IsNullOrEmpty(css)) return result;

        foreach (var pair in css.Split(';', StringSplitOptions.RemoveEmptyEntries))
        {
            var kv = pair.Split(':', 2);
            if (kv.Length == 2)
                result[kv[0].Trim()] = kv[1].Trim();
        }
        return result;
    }

    private static JustificationValues MapearAlineacion(string align) => align switch
    {
        "center" => JustificationValues.Center,
        "right" => JustificationValues.Right,
        "justify" => JustificationValues.Both,
        _ => JustificationValues.Left
    };

    private static string ConvertirPtAHalfPt(string pt)
    {
        var match = Regex.Match(pt, @"([\d.]+)");
        if (match.Success && double.TryParse(match.Groups[1].Value, out var val))
            return ((int)(val * 2)).ToString();
        return "20";
    }

    private static int ConvertirPtATwips(string value)
    {
        var match = Regex.Match(value, @"([\d.]+)\s*(pt|in|)");
        if (!match.Success || !double.TryParse(match.Groups[1].Value, out var num))
            return 0;

        return match.Groups[2].Value switch
        {
            "in" => (int)(num * 1440),
            "pt" => (int)(num * 20),
            _ => (int)(num * 20)
        };
    }

    private static int ConvertirInchATwips(string value)
    {
        var match = Regex.Match(value, @"([\d.]+)\s*(in|pt|)");
        if (!match.Success || !double.TryParse(match.Groups[1].Value, out var num))
            return 0;

        return match.Groups[2].Value switch
        {
            "in" => (int)(num * 1440),
            "pt" => (int)(num * 20),
            _ => (int)(num * 1440)
        };
    }

    private static string? ExtraerAncho(Dictionary<string, string> style)
    {
        return style.TryGetValue("width", out var w) ? w : null;
    }
}
