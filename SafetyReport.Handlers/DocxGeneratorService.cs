using System.Text.RegularExpressions;
using System.Text.Json.Nodes;
using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Wordprocessing;
using A = DocumentFormat.OpenXml.Drawing;
using DW = DocumentFormat.OpenXml.Drawing.Wordprocessing;
using PIC = DocumentFormat.OpenXml.Drawing.Pictures;

namespace SafetyReport.Handlers;

public partial class DocxGeneratorService
{
    private string _fontFamily = "Calibri";
    private int _fontSizeHp = 20;
    private int _lineSpacing = 276;
    private int _contentIndentL = 0;
    private int _contentIndentR = 0;
    private int _contentWidth = 0; // available width for content in twips
    private byte[]? _logoBytes;

    public MemoryStream GenerarDocx(JsonNode json, byte[]? logoBytes = null)
    {
        var ms = new MemoryStream();
        using (var doc = WordprocessingDocument.Create(ms, WordprocessingDocumentType.Document, true))
        {
            var mainPart = doc.AddMainDocumentPart();
            mainPart.Document = new Document();
            var body = new Body();

            var config = json["document"];
            var sections = json["sections"]?.AsArray();

            _logoBytes = logoBytes;
            LeerConfigGlobal(config);

            if (sections != null)
                foreach (var section in sections)
                    if (section != null) RenderizarSeccion(body, section);

            AgregarHeaderLogo(mainPart, config);
            AgregarFooter(mainPart, config);
            AgregarSectionProperties(body, mainPart, config);

            mainPart.Document.Append(body);
            mainPart.Document.Save();
        }

        ms.Position = 0;
        return ms;
    }

    private void LeerConfigGlobal(JsonNode? config)
    {
        if (config is null) return;
        _fontFamily = config["font"]?["family"]?.GetValue<string>()?.Split(',')[0].Trim() ?? "Calibri";
        var fs = config["font"]?["size"]?.GetValue<string>() ?? "10pt";
        _fontSizeHp = PtToHalfPt(fs);
        var ls = config["font"]?["lineSpacing"]?.GetValue<double>() ?? 1.15;
        _lineSpacing = (int)(ls * 240);
        _contentIndentL = CssToTwips(config["contentIndent"]?["left"]?.GetValue<string>() ?? "0");
        _contentIndentR = CssToTwips(config["contentIndent"]?["right"]?.GetValue<string>() ?? "0");

        var pageW = CssToTwips(config["pageSize"]?["width"]?.GetValue<string>() ?? "8.27in");
        var ml = CssToTwips(config["margins"]?["left"]?.GetValue<string>() ?? "0.5in");
        var mr = CssToTwips(config["margins"]?["right"]?.GetValue<string>() ?? "0.5in");
        _contentWidth = pageW - ml - mr - _contentIndentL - _contentIndentR;
    }

    // ==================== SECTION RENDERERS ====================

    private void RenderizarSeccion(Body body, JsonNode section)
    {
        var type = section["type"]?.GetValue<string>() ?? "";
        switch (type)
        {
            case "heading": RenderHeading(body, section); break;
            case "subtitle": RenderSubtitle(body, section); break;
            case "text": RenderText(body, section); break;
            case "keyValue": RenderKeyValue(body, section); break;
            case "borderedBox": RenderBorderedBox(body, section); break;
            case "referenceBox": RenderReferenceBox(body, section); break;
            case "dataTable": RenderDataTable(body, section); break;
            case "repeat":
                var subs = section["sections"]?.AsArray();
                if (subs != null)
                    foreach (var sub in subs)
                        if (sub != null) RenderizarSeccion(body, sub);
                break;
            case "repeatDetail": RenderRepeatDetail(body, section); break;
            case "spacer": RenderSpacer(body, section); break;
        }
    }

    private void RenderHeading(Body body, JsonNode section)
    {
        var text = section["text"]?.GetValue<string>() ?? "";
        var css = ParseCss(section["style"]?.GetValue<string>());

        var para = new Paragraph();
        var pPr = CrearParagraphProps(css);
        para.Append(pPr);
        para.Append(CrearRun(text, css));
        body.Append(para);
    }

    private void RenderSubtitle(Body body, JsonNode section)
    {
        var text = section["text"]?.GetValue<string>() ?? "";
        var css = ParseCss(section["style"]?.GetValue<string>());

        var para = new Paragraph();
        var pPr = CrearParagraphProps(css);
        para.Append(pPr);
        para.Append(CrearRun(text, css));
        body.Append(para);
    }

    private void RenderText(Body body, JsonNode section)
    {
        var text = section["field"]?.GetValue<string>() ?? "";
        var css = ParseCss(section["style"]?.GetValue<string>());
        if (string.IsNullOrEmpty(text)) return;

        var lines = text.Split('\n');
        foreach (var line in lines)
        {
            var para = new Paragraph();
            var pPr = new ParagraphProperties();
            pPr.Append(new SpacingBetweenLines { After = "0", Line = _lineSpacing.ToString(), LineRule = LineSpacingRuleValues.Auto });
            AgregarIndentacion(pPr);
            para.Append(pPr);
            para.Append(CrearRun(line, css));
            body.Append(para);
        }
    }

    private void RenderKeyValue(Body body, JsonNode section)
    {
        var tblCss = ParseCss(section["style"]?.GetValue<string>());
        var lblCss = ParseCss(section["labelStyle"]?.GetValue<string>());
        var rows = section["rows"]?.AsArray();
        if (rows is null || rows.Count == 0) return;

        var table = CrearTabla(tblCss);
        var tblW = ObtenerAnchoTabla(tblCss);
        var lblW = CssToTwips(lblCss.GetValueOrDefault("width", ""));
        var valW = lblW > 0 && tblW > lblW ? tblW - lblW : 0;

        foreach (var row in rows)
        {
            if (row is null) continue;
            var label = row["label"]?.GetValue<string>() ?? "";
            var value = row["value"]?.GetValue<string>() ?? "";
            var sep = row["separator"]?.GetValue<string>() ?? "";

            var tr = new TableRow();
            tr.Append(CrearCeldaTexto(label, lblW, lblCss));
            tr.Append(CrearCeldaTexto(sep + value, valW, null));
            table.Append(tr);
        }

        AgregarTablaConMargen(body, table, tblCss);
    }

    private void RenderBorderedBox(Body body, JsonNode section)
    {
        var tblCss = ParseCss(section["style"]?.GetValue<string>());
        var titleCss = ParseCss(section["titleStyle"]?.GetValue<string>());
        var lblCss = ParseCss(section["labelStyle"]?.GetValue<string>());
        var valCss = ParseCss(section["valueStyle"]?.GetValue<string>());
        var cellCss = ParseCss(section["cellStyle"]?.GetValue<string>());
        var title = section["title"]?.GetValue<string>() ?? "";
        var content = section["content"]?.GetValue<string>();
        var rows = section["rows"]?.AsArray();

        var hasBorder = tblCss.ContainsKey("border") || cellCss.ContainsKey("border");
        var table = CrearTabla(tblCss, hasBorder);
        var tblW = ObtenerAnchoTabla(tblCss);
        var lblW = CssToTwips(lblCss.GetValueOrDefault("width", ""));
        var valW = lblW > 0 && tblW > lblW ? tblW - lblW : 0;

        // Title row (colspan 2)
        var trTitle = new TableRow();
        trTitle.Append(CrearCeldaTexto(title, tblW, titleCss, colspan: 2));
        table.Append(trTitle);

        // Content row
        if (!string.IsNullOrEmpty(content))
        {
            var trContent = new TableRow();
            trContent.Append(CrearCeldaTexto(content, tblW, cellCss, colspan: 2));
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
                var rowCss = ParseCss(row["style"]?.GetValue<string>());
                var effectiveLblCss = rowCss.Count > 0 ? rowCss : lblCss;
                var effectiveLblW = CssToTwips(rowCss.GetValueOrDefault("width", ""));
                if (effectiveLblW == 0) effectiveLblW = lblW;
                var effectiveValW = effectiveLblW > 0 && tblW > effectiveLblW ? tblW - effectiveLblW : valW;

                var tr = new TableRow();
                tr.Append(CrearCeldaTexto(label, effectiveLblW, effectiveLblCss));
                tr.Append(CrearCeldaTexto(value, effectiveValW, valCss));
                table.Append(tr);
            }
        }

        AgregarTablaConMargen(body, table, tblCss);
    }

    private void RenderReferenceBox(Body body, JsonNode section)
    {
        var tblCss = ParseCss(section["style"]?.GetValue<string>());
        var titleCss = ParseCss(section["titleStyle"]?.GetValue<string>());
        var cellCss = ParseCss(section["cellStyle"]?.GetValue<string>());
        var lastCellCss = ParseCss(section["lastCellStyle"]?.GetValue<string>());
        var title = section["title"]?.GetValue<string>() ?? "";
        var items = section["items"]?.AsArray();

        var table = CrearTabla(tblCss, true);

        var trTitle = new TableRow();
        trTitle.Append(CrearCeldaTexto(title, 0, titleCss));
        table.Append(trTitle);

        if (items != null)
        {
            for (int i = 0; i < items.Count; i++)
            {
                var text = items[i]?.GetValue<string>() ?? "";
                var isLast = i == items.Count - 1;
                var css = isLast && lastCellCss.Count > 0 ? lastCellCss : cellCss;

                var tr = new TableRow();
                tr.Append(CrearCeldaTexto(text, 0, css, fontSize: CssToHalfPt(tblCss.GetValueOrDefault("font-size", ""))));
                table.Append(tr);
            }
        }

        AgregarTablaConMargen(body, table, tblCss);
    }

    private void RenderDataTable(Body body, JsonNode section)
    {
        var tblCss = ParseCss(section["style"]?.GetValue<string>());
        var cellCss = ParseCss(section["cellStyle"]?.GetValue<string>());
        var headerCss = ParseCss(section["headerStyle"]?.GetValue<string>());
        var columns = section["columns"]?.AsArray();
        var rows = section["rows"]?.AsArray();
        var columnWidths = section["columnWidths"]?.AsArray();

        if (columns is null) return;

        var table = CrearTabla(tblCss);

        // Merge cell + header styles for header cells
        var mergedHeaderCss = new Dictionary<string, string>(cellCss);
        foreach (var kv in headerCss) mergedHeaderCss[kv.Key] = kv.Value;

        // Header row
        var trHeader = new TableRow();
        for (int i = 0; i < columns.Count; i++)
        {
            var header = columns[i]?["header"]?.GetValue<string>() ?? "";
            var w = columnWidths != null && i < columnWidths.Count ? CssToTwips(columnWidths[i]?.GetValue<string>() ?? "") : 0;
            trHeader.Append(CrearCeldaTexto(header, w, mergedHeaderCss));
        }
        table.Append(trHeader);

        // Data rows
        if (rows != null)
        {
            foreach (var row in rows)
            {
                if (row is null) continue;
                var tr = new TableRow();
                var values = new List<string>();

                if (row is JsonArray arr)
                    foreach (var v in arr) values.Add(v?.GetValue<string>() ?? "");
                else if (row is JsonObject obj)
                    foreach (var prop in obj) values.Add(prop.Value?.GetValue<string>() ?? "");

                for (int i = 0; i < values.Count; i++)
                {
                    var w = columnWidths != null && i < columnWidths.Count ? CssToTwips(columnWidths[i]?.GetValue<string>() ?? "") : 0;
                    tr.Append(CrearCeldaTexto(values[i], w, cellCss));
                }
                table.Append(tr);
            }
        }

        AgregarTablaConMargen(body, table, tblCss);
    }

    private void RenderRepeatDetail(Body body, JsonNode section)
    {
        var titleCss = ParseCss(section["titleStyle"]?.GetValue<string>());
        var contentCss = ParseCss(section["contentStyle"]?.GetValue<string>());
        var items = section["items"]?.AsArray();
        if (items is null) return;

        foreach (var item in items)
        {
            if (item is null) continue;
            var title = item["title"]?.GetValue<string>() ?? "";
            var content = item["content"]?.GetValue<string>() ?? "";

            var pTitle = new Paragraph();
            pTitle.Append(CrearParagraphProps(titleCss));
            pTitle.Append(CrearRun(title, titleCss));
            body.Append(pTitle);

            if (!string.IsNullOrEmpty(content))
            {
                foreach (var line in content.Split('\n'))
                {
                    var pContent = new Paragraph();
                    var pPr = new ParagraphProperties();
                    pPr.Append(new SpacingBetweenLines { After = "0", Line = _lineSpacing.ToString(), LineRule = LineSpacingRuleValues.Auto });
                    AgregarIndentacion(pPr);
                    pContent.Append(pPr);
                    pContent.Append(CrearRun(line, contentCss));
                    body.Append(pContent);
                }
            }
        }
    }

    private void RenderSpacer(Body body, JsonNode section)
    {
        var height = section["height"]?.GetValue<string>() ?? "0.3in";
        var para = new Paragraph();
        var pPr = new ParagraphProperties();
        pPr.Append(new SpacingBetweenLines { Before = CssToTwips(height).ToString(), After = "0" });
        para.Append(pPr);
        body.Append(para);
    }

    // ==================== HEADER / FOOTER / PAGE ====================

    private void AgregarHeaderLogo(MainDocumentPart mainPart, JsonNode? config)
    {
        if (_logoBytes is null || _logoBytes.Length == 0) return;

        var headerPart = mainPart.AddNewPart<HeaderPart>();
        var header = new Header();

        var logoW = CssToEmu(config?["header"]?["logoWidth"]?.GetValue<string>() ?? "1.3in");
        var logoH = CssToEmu(config?["header"]?["logoHeight"]?.GetValue<string>() ?? "0.55in");
        var align = config?["header"]?["align"]?.GetValue<string>() ?? "center";

        var imagePart = headerPart.AddImagePart(ImagePartType.Png);
        using (var imgStream = new MemoryStream(_logoBytes))
            imagePart.FeedData(imgStream);

        var relationshipId = headerPart.GetIdOfPart(imagePart);

        var drawing = new Drawing(
            new DW.Inline(
                new DW.Extent { Cx = logoW, Cy = logoH },
                new DW.EffectExtent { LeftEdge = 0, TopEdge = 0, RightEdge = 0, BottomEdge = 0 },
                new DW.DocProperties { Id = 1, Name = "Logo" },
                new DW.NonVisualGraphicFrameDrawingProperties(
                    new A.GraphicFrameLocks { NoChangeAspect = true }),
                new A.Graphic(
                    new A.GraphicData(
                        new PIC.Picture(
                            new PIC.NonVisualPictureProperties(
                                new PIC.NonVisualDrawingProperties { Id = 0, Name = "logo.png" },
                                new PIC.NonVisualPictureDrawingProperties()),
                            new PIC.BlipFill(
                                new A.Blip { Embed = relationshipId },
                                new A.Stretch(new A.FillRectangle())),
                            new PIC.ShapeProperties(
                                new A.Transform2D(
                                    new A.Offset { X = 0, Y = 0 },
                                    new A.Extents { Cx = logoW, Cy = logoH }),
                                new A.PresetGeometry(new A.AdjustValueList()) { Preset = A.ShapeTypeValues.Rectangle }))
                    ) { Uri = "http://schemas.openxmlformats.org/drawingml/2006/picture" })
            ) { DistanceFromTop = 0, DistanceFromBottom = 0, DistanceFromLeft = 0, DistanceFromRight = 0 });

        var para = new Paragraph();
        var pPr = new ParagraphProperties();
        pPr.Append(new Justification { Val = MapAlign(align) });
        para.Append(pPr);

        var run = new Run();
        run.Append(drawing);
        para.Append(run);

        header.Append(para);
        headerPart.Header = header;
        headerPart.Header.Save();
    }

    private void AgregarFooter(MainDocumentPart mainPart, JsonNode? config)
    {
        var footerText = config?["footer"]?["text"]?.GetValue<string>();
        if (string.IsNullOrEmpty(footerText)) return;

        var footerPart = mainPart.AddNewPart<FooterPart>();
        var footer = new Footer();
        var footerFontSize = PtToHalfPt(config?["footer"]?["fontSize"]?.GetValue<string>() ?? "7pt").ToString();
        var footerAlign = config?["footer"]?["align"]?.GetValue<string>() ?? "left";
        var fiL = CssToTwips(config?["footerIndent"]?["left"]?.GetValue<string>() ?? "0");
        var fiR = CssToTwips(config?["footerIndent"]?["right"]?.GetValue<string>() ?? "0");

        // Footer text
        var para = new Paragraph();
        var pPr = new ParagraphProperties();
        pPr.Append(new Justification { Val = MapAlign(footerAlign) });
        pPr.Append(new SpacingBetweenLines { After = "0", Line = "240" });
        if (fiL > 0 || fiR > 0)
            pPr.Append(new Indentation { Left = fiL.ToString(), Right = fiR.ToString() });
        para.Append(pPr);

        var run = new Run();
        run.Append(new RunProperties(new FontSize { Val = footerFontSize }, new RunFonts { Ascii = _fontFamily, HighAnsi = _fontFamily }));
        run.Append(new Text(footerText) { Space = SpaceProcessingModeValues.Preserve });
        para.Append(run);
        footer.Append(para);

        // Page number
        var paraPage = new Paragraph();
        var pPrPage = new ParagraphProperties();
        pPrPage.Append(new Justification { Val = MapAlign(footerAlign) });
        if (fiL > 0 || fiR > 0)
            pPrPage.Append(new Indentation { Left = fiL.ToString(), Right = fiR.ToString() });
        paraPage.Append(pPrPage);

        var pageLabel = config?["footer"]?["pageLabel"]?.GetValue<string>() ?? "Page";
        var runPage = new Run();
        runPage.Append(new RunProperties(new FontSize { Val = footerFontSize }, new RunFonts { Ascii = _fontFamily, HighAnsi = _fontFamily }));
        runPage.Append(new Text(pageLabel + " ") { Space = SpaceProcessingModeValues.Preserve });
        paraPage.Append(runPage);

        var runField = new Run();
        runField.Append(new RunProperties(new FontSize { Val = footerFontSize }, new RunFonts { Ascii = _fontFamily, HighAnsi = _fontFamily }));
        runField.Append(new FieldChar { FieldCharType = FieldCharValues.Begin });
        paraPage.Append(runField);

        var runCode = new Run();
        runCode.Append(new RunProperties(new FontSize { Val = footerFontSize }, new RunFonts { Ascii = _fontFamily, HighAnsi = _fontFamily }));
        runCode.Append(new FieldCode(" PAGE ") { Space = SpaceProcessingModeValues.Preserve });
        paraPage.Append(runCode);

        var runEnd = new Run();
        runEnd.Append(new RunProperties(new FontSize { Val = footerFontSize }, new RunFonts { Ascii = _fontFamily, HighAnsi = _fontFamily }));
        runEnd.Append(new FieldChar { FieldCharType = FieldCharValues.End });
        paraPage.Append(runEnd);

        footer.Append(paraPage);

        footerPart.Footer = footer;
        footerPart.Footer.Save();
    }

    private void AgregarSectionProperties(Body body, MainDocumentPart mainPart, JsonNode? config)
    {
        if (config is null) return;

        var pageW = CssToTwips(config["pageSize"]?["width"]?.GetValue<string>() ?? "8.27in");
        var pageH = CssToTwips(config["pageSize"]?["height"]?.GetValue<string>() ?? "11.69in");
        var mt = CssToTwips(config["margins"]?["top"]?.GetValue<string>() ?? "1.15in");
        var mb = CssToTwips(config["margins"]?["bottom"]?.GetValue<string>() ?? "1.0in");
        var ml = CssToTwips(config["margins"]?["left"]?.GetValue<string>() ?? "0.5in");
        var mr = CssToTwips(config["margins"]?["right"]?.GetValue<string>() ?? "0.5in");

        var secPr = new SectionProperties();
        secPr.Append(new PageSize { Width = (uint)pageW, Height = (uint)pageH });
        secPr.Append(new PageMargin { Top = mt, Bottom = mb, Left = (uint)ml, Right = (uint)mr });

        // Link header
        var headerPart = mainPart.HeaderParts.FirstOrDefault();
        if (headerPart != null)
            secPr.InsertAt(new HeaderReference { Type = HeaderFooterValues.Default, Id = mainPart.GetIdOfPart(headerPart) }, 0);

        // Link footer
        var footerPart = mainPart.FooterParts.FirstOrDefault();
        if (footerPart != null)
            secPr.InsertAt(new FooterReference { Type = HeaderFooterValues.Default, Id = mainPart.GetIdOfPart(footerPart) }, 0);

        body.Append(secPr);
    }

    private void AgregarTablaConMargen(Body body, Table table, Dictionary<string, string> tblCss)
    {
        int before = 0, after = 0;
        if (tblCss.TryGetValue("margin", out var margin))
        {
            var parts = margin.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length >= 1) before = CssToTwips(parts[0]);
            if (parts.Length >= 3) after = CssToTwips(parts[2]);
            else if (parts.Length == 2) after = 0;
        }
        if (tblCss.TryGetValue("margin-top", out var mt)) before = CssToTwips(mt);
        if (tblCss.TryGetValue("margin-bottom", out var mb)) after = CssToTwips(mb);

        if (before > 0)
        {
            var spacer = new Paragraph(new ParagraphProperties(
                new SpacingBetweenLines { Before = "0", After = before.ToString() }));
            body.Append(spacer);
        }

        body.Append(table);

        if (after > 0)
        {
            var spacer = new Paragraph(new ParagraphProperties(
                new SpacingBetweenLines { Before = after.ToString(), After = "0" }));
            body.Append(spacer);
        }
    }

    // ==================== ELEMENT BUILDERS ====================

    private ParagraphProperties CrearParagraphProps(Dictionary<string, string> css)
    {
        var pPr = new ParagraphProperties();

        if (css.TryGetValue("text-align", out var align))
            pPr.Append(new Justification { Val = MapAlign(align) });

        int before = 0, after = 0;

        if (css.TryGetValue("margin", out var margin))
        {
            var parts = margin.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length >= 1) before = CssToTwips(parts[0]);
            if (parts.Length >= 3) after = CssToTwips(parts[2]);
        }
        if (css.TryGetValue("padding-top", out var pt)) before = CssToTwips(pt);
        if (css.TryGetValue("padding-bottom", out var pb)) after = CssToTwips(pb);
        if (css.TryGetValue("margin-top", out var mt)) before = CssToTwips(mt);
        if (css.TryGetValue("margin-bottom", out var mb)) after = CssToTwips(mb);

        pPr.Append(new SpacingBetweenLines
        {
            Before = before.ToString(),
            After = after.ToString(),
            Line = _lineSpacing.ToString(),
            LineRule = LineSpacingRuleValues.Auto
        });

        AgregarIndentacion(pPr);

        return pPr;
    }

    private Run CrearRun(string text, Dictionary<string, string> css)
    {
        var run = new Run();
        var rPr = new RunProperties();

        var hp = _fontSizeHp;
        if (css.TryGetValue("font-size", out var fs))
            hp = PtToHalfPt(fs);

        rPr.Append(new FontSize { Val = hp.ToString() });
        rPr.Append(new RunFonts { Ascii = _fontFamily, HighAnsi = _fontFamily, ComplexScript = _fontFamily });

        if (css.GetValueOrDefault("font-weight") is "700" or "bold")
            rPr.Append(new Bold());

        run.Append(rPr);
        run.Append(new Text(text) { Space = SpaceProcessingModeValues.Preserve });
        return run;
    }

    private Table CrearTabla(Dictionary<string, string> css, bool allBorders = false)
    {
        var table = new Table();
        var tPr = new TableProperties();
        var isFixed = false;

        // Width — convert % to absolute twips using calculated content width
        if (css.TryGetValue("width", out var w))
        {
            if (w.Contains('%'))
            {
                var m = Regex.Match(w, @"([\d.]+)");
                var pct = m.Success && double.TryParse(m.Groups[1].Value, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out var p) ? p / 100.0 : 1.0;
                var twips = (int)(_contentWidth * pct);
                tPr.Append(new TableWidth { Width = twips.ToString(), Type = TableWidthUnitValues.Dxa });
                isFixed = true;
            }
            else
            {
                tPr.Append(new TableWidth { Width = CssToTwips(w).ToString(), Type = TableWidthUnitValues.Dxa });
                isFixed = true;
            }
        }
        else
        {
            tPr.Append(new TableWidth { Width = _contentWidth.ToString(), Type = TableWidthUnitValues.Dxa });
            isFixed = true;
        }

        // Centering
        if (css.TryGetValue("margin", out var margin) && margin.Contains("auto"))
            tPr.Append(new TableJustification { Val = TableRowAlignmentValues.Center });

        // Borders
        if (allBorders || css.ContainsKey("border"))
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

        // Cell margins (matching CSS: td, th { padding: 0 0.03in })
        tPr.Append(new TableCellMarginDefault(
            new TableCellLeftMargin { Width = 43, Type = TableWidthValues.Dxa },
            new TableCellRightMargin { Width = 43, Type = TableWidthValues.Dxa }
        ));

        if (isFixed)
            tPr.Append(new TableLayout { Type = TableLayoutValues.Fixed });

        // Indentation to match content indent
        if (_contentIndentL > 0 || _contentIndentR > 0)
            tPr.Append(new TableIndentation { Width = _contentIndentL, Type = TableWidthUnitValues.Dxa });

        table.Append(tPr);
        return table;
    }

    private TableCell CrearCeldaTexto(string text, int widthTwips, Dictionary<string, string>? css,
        int colspan = 0, int fontSize = 0)
    {
        var tc = new TableCell();
        var tcPr = new TableCellProperties();

        if (widthTwips > 0)
            tcPr.Append(new TableCellWidth { Width = widthTwips.ToString(), Type = TableWidthUnitValues.Dxa });

        if (colspan > 1)
            tcPr.Append(new GridSpan { Val = colspan });

        // Cell borders from style
        if (css != null && css.ContainsKey("border"))
        {
            tcPr.Append(new TableCellBorders(
                new TopBorder { Val = BorderValues.Single, Size = 4, Space = 0, Color = "000000" },
                new BottomBorder { Val = BorderValues.Single, Size = 4, Space = 0, Color = "000000" },
                new LeftBorder { Val = BorderValues.Single, Size = 4, Space = 0, Color = "000000" },
                new RightBorder { Val = BorderValues.Single, Size = 4, Space = 0, Color = "000000" }
            ));
        }
        else if (css != null)
        {
            var borders = new TableCellBorders();
            if (css.ContainsKey("border-bottom")) borders.Append(new BottomBorder { Val = BorderValues.Single, Size = 4, Space = 0, Color = "000000" });
            if (css.ContainsKey("border-top")) borders.Append(new TopBorder { Val = BorderValues.Single, Size = 4, Space = 0, Color = "000000" });
            if (css.ContainsKey("border-left")) borders.Append(new LeftBorder { Val = BorderValues.Single, Size = 4, Space = 0, Color = "000000" });
            if (css.ContainsKey("border-right")) borders.Append(new RightBorder { Val = BorderValues.Single, Size = 4, Space = 0, Color = "000000" });
            if (borders.HasChildren)
                tcPr.Append(borders);
        }

        tcPr.Append(new TableCellVerticalAlignment { Val = TableVerticalAlignmentValues.Top });
        tc.Append(tcPr);

        // Paragraph inside cell
        var para = new Paragraph();
        var pPr = new ParagraphProperties();
        pPr.Append(new SpacingBetweenLines { After = "0", Line = "240" });

        if (css != null && css.TryGetValue("text-align", out var align))
            pPr.Append(new Justification { Val = MapAlign(align) });

        para.Append(pPr);

        // Run
        var run = new Run();
        var rPr = new RunProperties();

        var hp = fontSize > 0 ? fontSize : (css != null && css.TryGetValue("font-size", out var fs) ? PtToHalfPt(fs) : _fontSizeHp);
        rPr.Append(new FontSize { Val = hp.ToString() });
        rPr.Append(new RunFonts { Ascii = _fontFamily, HighAnsi = _fontFamily, ComplexScript = _fontFamily });

        if (css != null && css.GetValueOrDefault("font-weight") is "700" or "bold")
            rPr.Append(new Bold());

        run.Append(rPr);

        // Multi-line support
        var lines = text.Split('\n');
        for (int i = 0; i < lines.Length; i++)
        {
            if (i > 0) run.Append(new Break());
            run.Append(new Text(lines[i]) { Space = SpaceProcessingModeValues.Preserve });
        }

        para.Append(run);
        tc.Append(para);
        return tc;
    }

    private void AgregarIndentacion(ParagraphProperties pPr)
    {
        if (_contentIndentL > 0 || _contentIndentR > 0)
            pPr.Append(new Indentation { Left = _contentIndentL.ToString(), Right = _contentIndentR.ToString() });
    }

    private int ObtenerAnchoTabla(Dictionary<string, string> css)
    {
        if (!css.TryGetValue("width", out var w)) return _contentWidth;
        if (w.Contains('%'))
        {
            var m = Regex.Match(w, @"([\d.]+)");
            var pct = m.Success && double.TryParse(m.Groups[1].Value, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out var p) ? p / 100.0 : 1.0;
            return (int)(_contentWidth * pct);
        }
        return CssToTwips(w);
    }

    // ==================== CSS PARSING / CONVERSION ====================

    private static Dictionary<string, string> ParseCss(string? css)
    {
        var result = new Dictionary<string, string>();
        if (string.IsNullOrEmpty(css)) return result;
        foreach (var pair in css.Split(';', StringSplitOptions.RemoveEmptyEntries))
        {
            var kv = pair.Split(':', 2);
            if (kv.Length == 2) result[kv[0].Trim()] = kv[1].Trim();
        }
        return result;
    }

    private static JustificationValues MapAlign(string align) => align switch
    {
        "center" => JustificationValues.Center,
        "right" => JustificationValues.Right,
        "justify" => JustificationValues.Both,
        _ => JustificationValues.Left
    };

    private static int PtToHalfPt(string value)
    {
        var m = Regex.Match(value, @"([\d.]+)");
        return m.Success && double.TryParse(m.Groups[1].Value, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out var v)
            ? (int)(v * 2)
            : 20;
    }

    private static int CssToHalfPt(string value)
    {
        if (string.IsNullOrEmpty(value)) return 0;
        return PtToHalfPt(value);
    }

    private static int CssToTwips(string value)
    {
        if (string.IsNullOrEmpty(value)) return 0;
        var m = Regex.Match(value, @"([\d.]+)\s*(in|pt|)");
        if (!m.Success || !double.TryParse(m.Groups[1].Value, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out var num))
            return 0;
        return m.Groups[2].Value switch
        {
            "in" => (int)(num * 1440),
            "pt" => (int)(num * 20),
            _ => (int)(num * 1440)
        };
    }

    private static long CssToEmu(string value)
    {
        var m = Regex.Match(value, @"([\d.]+)\s*(in|pt|)");
        if (!m.Success || !double.TryParse(m.Groups[1].Value, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out var num))
            return 0;
        return m.Groups[2].Value switch
        {
            "in" => (long)(num * 914400),
            "pt" => (long)(num * 12700),
            _ => (long)(num * 914400)
        };
    }
}
