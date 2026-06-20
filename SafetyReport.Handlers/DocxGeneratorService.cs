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
    private double _lineSpacingMultiplier = 1.15;
    private int _contentIndentL = 0;
    private int _contentIndentR = 0;
    private int _contentWidth = 0; // available width for content in twips
    private byte[]? _logoBytes;
    private int _pendingTableBottomMargin;
    private Paragraph? _lastBodyParagraph;
    private int _lastBodyMarginBottom;
    private int _lastBodyPaddingBottom;

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
            _pendingTableBottomMargin = 0;
            _lastBodyParagraph = null;
            _lastBodyMarginBottom = 0;
            _lastBodyPaddingBottom = 0;
            LeerConfigGlobal(config);

            if (sections != null)
                foreach (var section in sections)
                    if (section != null) RenderizarSeccion(body, section);

            FlushPendingTableMargin(body);

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
        if (ls > 10) ls = ls / 100.0;
        _lineSpacingMultiplier = ls;
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
        var pPr = new ParagraphProperties();

        if (css.TryGetValue("text-align", out var align))
            pPr.Append(new Justification { Val = MapAlign(align) });

        var (before, after) = ObtenerEspaciadoVertical(css);

        pPr.Append(new SpacingBetweenLines
        {
            Before = before.ToString(),
            After = after.ToString(),
            Line = CssLineSpacing(css).ToString(),
            LineRule = LineSpacingRuleValues.Exact
        });
        AplicarMargenPendienteTabla(pPr, css);

        AgregarIndentacion(pPr);
        para.Append(pPr);
        para.Append(CrearRun(text, css));
        body.Append(para);
        RegistrarParrafoBody(para, css);
    }

    private void RenderSubtitle(Body body, JsonNode section)
    {
        var text = section["text"]?.GetValue<string>() ?? "";
        var css = ParseCss(section["style"]?.GetValue<string>());

        var para = new Paragraph();
        var pPr = new ParagraphProperties();

        if (css.TryGetValue("text-align", out var align))
            pPr.Append(new Justification { Val = MapAlign(align) });

        var (before, after) = ObtenerEspaciadoVertical(css);

        pPr.Append(new SpacingBetweenLines
        {
            Before = before.ToString(),
            After = after.ToString(),
            Line = CssLineSpacing(css).ToString(),
            LineRule = LineSpacingRuleValues.Exact
        });
        AplicarMargenPendienteTabla(pPr, css);

        AgregarIndentacion(pPr);
        para.Append(pPr);
        para.Append(CrearRun(text, css));
        body.Append(para);
        RegistrarParrafoBody(para, css);
    }

    private void RenderText(Body body, JsonNode section)
    {
        var text = section["field"]?.GetValue<string>() ?? "";
        var css = ParseCss(section["style"]?.GetValue<string>());
        if (string.IsNullOrEmpty(text)) return;

        var para = new Paragraph();
        var pPr = new ParagraphProperties();
        pPr.Append(new SpacingBetweenLines { After = "0", Line = CssLineSpacing(css).ToString(), LineRule = LineSpacingRuleValues.Exact });
        AplicarMargenPendienteTabla(pPr, css);
        AgregarIndentacion(pPr);
        para.Append(pPr);
        para.Append(CrearRunConSaltos(text, css));
        body.Append(para);
        RegistrarParrafoBody(para, css);
    }

    private void RenderKeyValue(Body body, JsonNode section)
    {
        var tblCss = ParseCss(section["style"]?.GetValue<string>());
        var lblCss = ParseCss(section["labelStyle"]?.GetValue<string>());
        var rows = section["rows"]?.AsArray();
        if (rows is null || rows.Count == 0) return;

        var table = CrearTabla(tblCss);
        var effectiveLblCss = CombinarCssHeredable(tblCss, lblCss);
        var effectiveValCss = CombinarCssHeredable(tblCss, null);
        var tblW = ObtenerAnchoTabla(tblCss);
        var lblW = CssToTwips(lblCss.GetValueOrDefault("width", ""));
        var valW = lblW > 0 && tblW > lblW ? tblW - lblW : 0;

        foreach (var row in rows)
        {
            if (row is null) continue;
            var label = row["label"]?.GetValue<string>() ?? "";
            var value = row["value"]?.GetValue<string>() ?? "";
            var sep = row["separator"]?.GetValue<string>() ?? "";

            var tr = CrearFilaTabla();
            tr.Append(CrearCeldaTexto(label, lblW, effectiveLblCss));
            tr.Append(CrearCeldaTexto(sep + value, valW, effectiveValCss));
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

        var table = CrearTabla(tblCss);
        var tblW = ObtenerAnchoTabla(tblCss);
        var lblW = CssToTwips(lblCss.GetValueOrDefault("width", ""));
        var valW = lblW > 0 && tblW > lblW ? tblW - lblW : 0;

        // Title row (colspan 2)
        var trTitle = CrearFilaTabla();
        trTitle.Append(CrearCeldaTexto(title, tblW, CombinarCssHeredable(tblCss, titleCss), colspan: 2));
        table.Append(trTitle);

        // Content row
        if (!string.IsNullOrEmpty(content))
        {
            var trContent = CrearFilaTabla();
            trContent.Append(CrearCeldaTexto(content, tblW, CombinarCssHeredable(tblCss, cellCss), colspan: 2));
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
                var rawLblCss = rowCss.Count > 0 ? rowCss : lblCss;
                var effectiveLblCss = CombinarCssHeredable(tblCss, rawLblCss);
                var effectiveValCss = CombinarCssHeredable(tblCss, valCss);
                var effectiveLblW = CssToTwips(rowCss.GetValueOrDefault("width", ""));
                if (effectiveLblW == 0) effectiveLblW = lblW;
                var effectiveValW = effectiveLblW > 0 && tblW > effectiveLblW ? tblW - effectiveLblW : valW;

                var tr = CrearFilaTabla();
                tr.Append(CrearCeldaTexto(label, effectiveLblW, effectiveLblCss));
                tr.Append(CrearCeldaTexto(value, effectiveValW, effectiveValCss));
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

        var table = CrearTabla(tblCss);

        var trTitle = CrearFilaTabla();
        trTitle.Append(CrearCeldaTexto(title, 0, CombinarCssHeredable(tblCss, titleCss)));
        table.Append(trTitle);

        if (items != null)
        {
            for (int i = 0; i < items.Count; i++)
            {
                var text = items[i]?.GetValue<string>() ?? "";
                var isLast = i == items.Count - 1;
                var rawCss = isLast && lastCellCss.Count > 0 ? lastCellCss : cellCss;
                var css = CombinarCssHeredable(tblCss, rawCss);

                var tr = CrearFilaTabla();
                tr.Append(CrearCeldaTexto(text, 0, css));
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
        var effectiveCellCss = CombinarCssHeredable(tblCss, cellCss);
        var mergedHeaderCss = new Dictionary<string, string>(effectiveCellCss)
        {
            ["font-weight"] = "bold",
            ["text-align"] = "center"
        };
        foreach (var kv in headerCss) mergedHeaderCss[kv.Key] = kv.Value;

        // Header row
        var trHeader = CrearFilaTabla(header: true);
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
                var tr = CrearFilaTabla();
                var values = new List<string>();

                if (row is JsonArray arr)
                    foreach (var v in arr) values.Add(v?.GetValue<string>() ?? "");
                else if (row is JsonObject obj)
                    foreach (var prop in obj) values.Add(prop.Value?.GetValue<string>() ?? "");

                for (int i = 0; i < values.Count; i++)
                {
                    var w = columnWidths != null && i < columnWidths.Count ? CssToTwips(columnWidths[i]?.GetValue<string>() ?? "") : 0;
                    tr.Append(CrearCeldaTexto(values[i], w, effectiveCellCss));
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
            RegistrarParrafoBody(pTitle, titleCss);

            if (!string.IsNullOrEmpty(content))
            {
                var pContent = new Paragraph();
                var pPr = new ParagraphProperties();
                pPr.Append(new SpacingBetweenLines { After = "0", Line = CssLineSpacing(contentCss).ToString(), LineRule = LineSpacingRuleValues.Exact });
                AplicarMargenPendienteTabla(pPr, contentCss);
                AgregarIndentacion(pPr);
                pContent.Append(pPr);
                pContent.Append(CrearRunConSaltos(content, contentCss));
                body.Append(pContent);
                RegistrarParrafoBody(pContent, contentCss);
            }
        }
    }

    private void RenderSpacer(Body body, JsonNode section)
    {
        var height = section["height"]?.GetValue<string>() ?? "0.3in";
        FlushPendingTableMargin(body);
        body.Append(CrearParrafoEspaciador(CssToTwips(height)));
        LimpiarUltimoParrafoBody();
    }

    // ==================== HEADER / FOOTER / PAGE ====================

    private void AgregarHeaderLogo(MainDocumentPart mainPart, JsonNode? config)
    {
        if (_logoBytes is null || _logoBytes.Length == 0) return;

        var headerPart = mainPart.AddNewPart<HeaderPart>();
        var header = new Header();

        var logoBoxW = CssToEmu(config?["header"]?["logoWidth"]?.GetValue<string>() ?? "1.3in");
        var logoBoxH = CssToEmu(config?["header"]?["logoHeight"]?.GetValue<string>() ?? "0.55in");
        var (logoW, logoH) = AjustarImagenContain(_logoBytes, logoBoxW, logoBoxH);
        var align = config?["header"]?["align"]?.GetValue<string>() ?? "center";

        var imagePart = _logoBytes.Length >= 2 && _logoBytes[0] == 0xFF && _logoBytes[1] == 0xD8
            ? headerPart.AddImagePart(ImagePartType.Jpeg)
            : headerPart.AddImagePart(ImagePartType.Png);
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
        var gapAfter = CssToTwips(config?["header"]?["gapAfter"]?.GetValue<string>() ?? "0");
        var verticalPadding = Math.Max(0, (int)((logoBoxH - logoH) / 635 / 2));
        pPr.Append(new SpacingBetweenLines
        {
            Before = verticalPadding.ToString(),
            After = (gapAfter + verticalPadding).ToString()
        });
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
        var footerPart = mainPart.AddNewPart<FooterPart>();
        var footer = new Footer();
        var footerFontSizeHp = PtToHalfPt(config?["footer"]?["fontSize"]?.GetValue<string>() ?? "7pt");
        var footerFontSize = footerFontSizeHp.ToString();
        var footerLineHeight = (footerFontSizeHp * 10).ToString();
        var footerAlign = config?["footer"]?["align"]?.GetValue<string>() ?? "left";
        var fiL = CssToTwips(config?["footerIndent"]?["left"]?.GetValue<string>() ?? "0");
        var fiR = CssToTwips(config?["footerIndent"]?["right"]?.GetValue<string>() ?? "0");
        var gapBefore = CssToTwips(config?["footer"]?["gapBefore"]?.GetValue<string>() ?? "0");
        var showPageNumber = config?["footer"]?["showPageNumber"]?.GetValue<bool>() ?? true;
        var footerPageW = CssToTwips(config?["pageSize"]?["width"]?.GetValue<string>());
        var footerMl = CssToTwips(config?["margins"]?["left"]?.GetValue<string>());
        var footerMr = CssToTwips(config?["margins"]?["right"]?.GetValue<string>());
        var footerTableWidth = footerPageW - footerMl - footerMr;

        var footerTable = new Table();
        footerTable.Append(new TableProperties(
            new TableWidth { Width = footerTableWidth.ToString(), Type = TableWidthUnitValues.Dxa },
            new TableLayout { Type = TableLayoutValues.Fixed }));

        var footerBoxHeight = CssToTwips(config?["margins"]?["bottom"]?.GetValue<string>());

        var footerRow = new TableRow();
        footerRow.Append(new TableRowProperties(
            new TableRowHeight { Val = (uint)footerBoxHeight, HeightType = HeightRuleValues.Exact }));

        var footerCell = new TableCell();
        footerCell.Append(new TableCellProperties(
            new TableCellWidth { Width = "5000", Type = TableWidthUnitValues.Pct },
            new TableCellMargin(
                new TopMargin { Width = "0", Type = TableWidthUnitValues.Dxa },
                new BottomMargin { Width = "0", Type = TableWidthUnitValues.Dxa },
                new LeftMargin { Width = fiL.ToString(), Type = TableWidthUnitValues.Dxa },
                new RightMargin { Width = fiR.ToString(), Type = TableWidthUnitValues.Dxa }),
            new TableCellVerticalAlignment { Val = TableVerticalAlignmentValues.Top }));

        // Footer text
        if (!string.IsNullOrEmpty(footerText))
        {
            var para = new Paragraph();
            var pPr = new ParagraphProperties();
            pPr.Append(new Justification { Val = MapAlign(footerAlign) });
            pPr.Append(new SpacingBetweenLines { Before = "0", After = "0", Line = footerLineHeight, LineRule = LineSpacingRuleValues.Exact });
            para.Append(pPr);

            var run = new Run();
            run.Append(new RunProperties(new FontSize { Val = footerFontSize }, new RunFonts { Ascii = _fontFamily, HighAnsi = _fontFamily }));
            run.Append(new Text(footerText) { Space = SpaceProcessingModeValues.Preserve });
            para.Append(run);
            footerCell.Append(para);
        }

        if (showPageNumber)
        {
            var paraPage = new Paragraph();
            var pPrPage = new ParagraphProperties();
            pPrPage.Append(new Justification { Val = MapAlign(footerAlign) });
            pPrPage.Append(new SpacingBetweenLines { Before = "0", After = "0", Line = footerLineHeight, LineRule = LineSpacingRuleValues.Exact });
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

            footerCell.Append(paraPage);
        }

        if (!footerCell.Elements<Paragraph>().Any())
            footerCell.Append(new Paragraph());

        footerRow.Append(footerCell);
        footerTable.Append(footerRow);
        footer.Append(footerTable);

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
        var logoHeight = CssToTwips(config["header"]?["logoHeight"]?.GetValue<string>() ?? "0.55in");
        var headerGap = CssToTwips(config["header"]?["gapAfter"]?.GetValue<string>() ?? "0");
        var headerDistance = Math.Max(0, mt - logoHeight - headerGap);
        var footerGapBefore = CssToTwips(config["footer"]?["gapBefore"]?.GetValue<string>() ?? "0");

        var secPr = new SectionProperties();
        secPr.Append(new PageSize { Width = (uint)pageW, Height = (uint)pageH });
        secPr.Append(new PageMargin
        {
            Top = mt,
            Bottom = mb - footerGapBefore * 7 / 8,
            Left = (uint)ml,
            Right = (uint)mr,
            Header = (uint)headerDistance,
            Footer = 0
        });

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
        var (before, after) = ObtenerMargenVertical(tblCss);

        if (_pendingTableBottomMargin > 0)
        {
            before = Math.Max(_pendingTableBottomMargin, before);
            _pendingTableBottomMargin = 0;
        }
        else if (_lastBodyParagraph != null && ReferenceEquals(body.LastChild, _lastBodyParagraph))
        {
            var spacing = _lastBodyParagraph
                .GetFirstChild<ParagraphProperties>()?
                .GetFirstChild<SpacingBetweenLines>();
            if (spacing != null)
                spacing.After = (_lastBodyPaddingBottom + Math.Max(_lastBodyMarginBottom, before)).ToString();
            before = 0;
        }

        if (before > 0)
            body.Append(CrearParrafoEspaciador(before));

        body.Append(table);
        _pendingTableBottomMargin = after;
        LimpiarUltimoParrafoBody();
    }

    // ==================== ELEMENT BUILDERS ====================

    private ParagraphProperties CrearParagraphProps(Dictionary<string, string> css)
    {
        var pPr = new ParagraphProperties();

        if (css.TryGetValue("text-align", out var align))
            pPr.Append(new Justification { Val = MapAlign(align) });

        var (before, after) = ObtenerEspaciadoVertical(css);

        pPr.Append(new SpacingBetweenLines
        {
            Before = before.ToString(),
            After = after.ToString(),
            Line = CssLineSpacing(css).ToString(),
            LineRule = LineSpacingRuleValues.Exact
        });
        AplicarMargenPendienteTabla(pPr, css);

        AgregarIndentacion(pPr);

        return pPr;
    }

    private Run CrearRun(string text, Dictionary<string, string> css)
    {
        var run = new Run();
        run.Append(CrearRunProperties(css));
        AgregarTextoAlRun(run, text, css, permitirSaltos: false);
        return run;
    }

    private Run CrearRunConSaltos(string text, Dictionary<string, string> css)
    {
        var run = new Run();
        run.Append(CrearRunProperties(css));
        AgregarTextoAlRun(run, text, css, permitirSaltos: true);
        return run;
    }

    private Table CrearTabla(Dictionary<string, string> css)
    {
        var table = new Table();
        var tPr = new TableProperties();
        var isFixed = false;
        var tableWidth = _contentWidth;

        // Width — convert % to absolute twips using calculated content width
        if (css.TryGetValue("width", out var w))
        {
            if (w.Contains('%'))
            {
                var m = Regex.Match(w, @"([\d.]+)");
                var pct = m.Success && double.TryParse(m.Groups[1].Value, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out var p) ? p / 100.0 : 1.0;
                tableWidth = (int)(_contentWidth * pct);
                tPr.Append(new TableWidth { Width = tableWidth.ToString(), Type = TableWidthUnitValues.Dxa });
                isFixed = true;
            }
            else
            {
                tableWidth = CssToTwips(w);
                tPr.Append(new TableWidth { Width = tableWidth.ToString(), Type = TableWidthUnitValues.Dxa });
                isFixed = true;
            }
        }
        else
        {
            tPr.Append(new TableWidth { Width = _contentWidth.ToString(), Type = TableWidthUnitValues.Dxa });
            isFixed = true;
        }

        var alignment = ObtenerAlineacionTabla(css);
        var remainingWidth = Math.Max(0, _contentWidth - tableWidth);
        var alignmentOffset = alignment == TableRowAlignmentValues.Center
            ? remainingWidth / 2
            : alignment == TableRowAlignmentValues.Right
                ? remainingWidth
                : 0;
        var tableIndent = _contentIndentL + alignmentOffset;
        tPr.Append(new TableJustification { Val = TableRowAlignmentValues.Left });
        tPr.Append(new TableIndentation { Width = tableIndent, Type = TableWidthUnitValues.Dxa });

        // Borders
        if (css.TryGetValue("border", out var tableBorder) && EsBordeVisible(tableBorder))
        {
            var (size, color) = ObtenerBorde(tableBorder);
            tPr.Append(new TableBorders(
                new TopBorder { Val = BorderValues.Single, Size = size, Space = 0, Color = color },
                new BottomBorder { Val = BorderValues.Single, Size = size, Space = 0, Color = color },
                new LeftBorder { Val = BorderValues.Single, Size = size, Space = 0, Color = color },
                new RightBorder { Val = BorderValues.Single, Size = size, Space = 0, Color = color }
            ));
        }

        if (isFixed)
            tPr.Append(new TableLayout { Type = TableLayoutValues.Fixed });


        table.Append(tPr);
        return table;
    }

    private TableCell CrearCeldaTexto(string text, int widthTwips, Dictionary<string, string>? css,
        int colspan = 0)
    {
        var tc = new TableCell();
        var tcPr = new TableCellProperties();

        if (widthTwips > 0)
            tcPr.Append(new TableCellWidth { Width = widthTwips.ToString(), Type = TableWidthUnitValues.Dxa });

        if (colspan > 1)
            tcPr.Append(new GridSpan { Val = colspan });

        // Cell borders from style
        if (css != null && css.TryGetValue("border", out var allBorder) && EsBordeVisible(allBorder))
        {
            var (size, color) = ObtenerBorde(allBorder);
            tcPr.Append(new TableCellBorders(
                new TopBorder { Val = BorderValues.Single, Size = size, Space = 0, Color = color },
                new BottomBorder { Val = BorderValues.Single, Size = size, Space = 0, Color = color },
                new LeftBorder { Val = BorderValues.Single, Size = size, Space = 0, Color = color },
                new RightBorder { Val = BorderValues.Single, Size = size, Space = 0, Color = color }
            ));
        }
        else if (css != null)
        {
            var borders = new TableCellBorders();
            if (css.TryGetValue("border-bottom", out var bottom) && EsBordeVisible(bottom))
            {
                var (size, color) = ObtenerBorde(bottom);
                borders.Append(new BottomBorder { Val = BorderValues.Single, Size = size, Space = 0, Color = color });
            }
            if (css.TryGetValue("border-top", out var top) && EsBordeVisible(top))
            {
                var (size, color) = ObtenerBorde(top);
                borders.Append(new TopBorder { Val = BorderValues.Single, Size = size, Space = 0, Color = color });
            }
            if (css.TryGetValue("border-left", out var left) && EsBordeVisible(left))
            {
                var (size, color) = ObtenerBorde(left);
                borders.Append(new LeftBorder { Val = BorderValues.Single, Size = size, Space = 0, Color = color });
            }
            if (css.TryGetValue("border-right", out var right) && EsBordeVisible(right))
            {
                var (size, color) = ObtenerBorde(right);
                borders.Append(new RightBorder { Val = BorderValues.Single, Size = size, Space = 0, Color = color });
            }
            if (borders.HasChildren)
                tcPr.Append(borders);
        }

        var (padTop, padRight, padBottom, padLeft) = ObtenerPadding(css);
        tcPr.Append(new TableCellMargin(
            new TopMargin { Width = padTop.ToString(), Type = TableWidthUnitValues.Dxa },
            new BottomMargin { Width = padBottom.ToString(), Type = TableWidthUnitValues.Dxa },
            new LeftMargin { Width = padLeft.ToString(), Type = TableWidthUnitValues.Dxa },
            new RightMargin { Width = padRight.ToString(), Type = TableWidthUnitValues.Dxa }
        ));

        tcPr.Append(new TableCellVerticalAlignment { Val = TableVerticalAlignmentValues.Top });
        tc.Append(tcPr);

        var hp = css != null && css.TryGetValue("font-size", out var fs) ? PtToHalfPt(fs) : _fontSizeHp;

        // Paragraph inside cell
        var para = new Paragraph();
        var pPr = new ParagraphProperties();
        pPr.Append(new SpacingBetweenLines
        {
            Before = "0",
            After = "0",
            Line = CssLineSpacing(css, hp).ToString(),
            LineRule = LineSpacingRuleValues.Exact
        });

        if (css != null && css.TryGetValue("text-align", out var align))
            pPr.Append(new Justification { Val = MapAlign(align) });

        para.Append(pPr);

        // Run
        var run = new Run();
        run.Append(CrearRunProperties(css));
        AgregarTextoAlRun(run, text, css, permitirSaltos: true);

        para.Append(run);
        tc.Append(para);
        return tc;
    }

    private TableRow CrearFilaTabla(bool header = false)
    {
        var row = new TableRow();
        var properties = new TableRowProperties(new CantSplit());
        if (header)
            properties.Append(new TableHeader());
        row.Append(properties);
        return row;
    }

    private RunProperties CrearRunProperties(Dictionary<string, string>? css)
    {
        var properties = new RunProperties();
        var hp = css != null && css.TryGetValue("font-size", out var fs) ? PtToHalfPt(fs) : _fontSizeHp;
        var family = ObtenerFamiliaFuente(css);

        properties.Append(new FontSize { Val = hp.ToString() });
        properties.Append(new RunFonts { Ascii = family, HighAnsi = family, ComplexScript = family });

        var weight = css?.GetValueOrDefault("font-weight");
        if (weight is "700" or "bold" || int.TryParse(weight, out var numericWeight) && numericWeight >= 600)
            properties.Append(new Bold());
        if (css?.GetValueOrDefault("font-style") is "italic" or "oblique")
            properties.Append(new Italic());
        if (css?.GetValueOrDefault("text-decoration")?.Contains("underline", StringComparison.OrdinalIgnoreCase) == true)
            properties.Append(new Underline { Val = UnderlineValues.Single });
        if (css != null && css.TryGetValue("color", out var color))
            properties.Append(new Color { Val = NormalizarColor(color) });

        return properties;
    }

    private void AgregarTextoAlRun(Run run, string text, Dictionary<string, string>? css, bool permitirSaltos)
    {
        var whiteSpace = css?.GetValueOrDefault("white-space")?.ToLowerInvariant() ?? "normal";
        var preservarSaltos = permitirSaltos && whiteSpace is "pre-line" or "pre-wrap" or "pre";

        if (!preservarSaltos)
        {
            var collapsed = Regex.Replace(text, @"\s+", " ").Trim();
            run.Append(new Text(collapsed) { Space = SpaceProcessingModeValues.Preserve });
            return;
        }

        var preservarEspacios = whiteSpace is "pre-wrap" or "pre";
        var lines = text.Replace("\r\n", "\n").Replace('\r', '\n').Split('\n');
        for (var i = 0; i < lines.Length; i++)
        {
            if (i > 0) run.Append(new Break());
            var line = preservarEspacios ? lines[i] : Regex.Replace(lines[i], @"[\t ]+", " ").Trim();
            run.Append(new Text(line) { Space = SpaceProcessingModeValues.Preserve });
        }
    }

    private Dictionary<string, string> CombinarCssHeredable(
        Dictionary<string, string> parent,
        Dictionary<string, string>? child)
    {
        string[] inheritedProperties =
        [
            "color", "font-family", "font-size", "font-style", "font-weight",
            "line-height", "text-align", "text-decoration", "white-space"
        ];

        var result = new Dictionary<string, string>();
        foreach (var property in inheritedProperties)
            if (parent.TryGetValue(property, out var value))
                result[property] = value;

        if (child != null)
            foreach (var pair in child)
                result[pair.Key] = pair.Value;

        return result;
    }

    private string ObtenerFamiliaFuente(Dictionary<string, string>? css)
    {
        if (css == null || !css.TryGetValue("font-family", out var value)) return _fontFamily;
        return value.Split(',')[0].Trim().Trim('\'', '"');
    }

    private static TableRowAlignmentValues ObtenerAlineacionTabla(Dictionary<string, string> css)
    {
        var margin = css.GetValueOrDefault("margin", "");
        var parts = margin.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        var horizontal = parts.Length switch
        {
            2 => parts[1],
            3 => parts[1],
            >= 4 when parts[1].Equals("auto", StringComparison.OrdinalIgnoreCase)
                && parts[3].Equals("auto", StringComparison.OrdinalIgnoreCase) => "auto",
            _ => ""
        };

        var leftAuto = css.GetValueOrDefault("margin-left", "").Equals("auto", StringComparison.OrdinalIgnoreCase);
        var rightAuto = css.GetValueOrDefault("margin-right", "").Equals("auto", StringComparison.OrdinalIgnoreCase);
        if (horizontal.Equals("auto", StringComparison.OrdinalIgnoreCase) || leftAuto && rightAuto)
            return TableRowAlignmentValues.Center;
        if (leftAuto)
            return TableRowAlignmentValues.Right;
        return TableRowAlignmentValues.Left;
    }

    private static bool EsBordeVisible(string value) =>
        !value.Contains("none", StringComparison.OrdinalIgnoreCase) && ObtenerBorde(value).Size > 0;

    private static (uint Size, string Color) ObtenerBorde(string value)
    {
        var widthMatch = Regex.Match(value, @"([\d.]+)\s*(px|pt)", RegexOptions.IgnoreCase);
        var size = 4u;
        if (widthMatch.Success && double.TryParse(widthMatch.Groups[1].Value,
                System.Globalization.NumberStyles.Any,
                System.Globalization.CultureInfo.InvariantCulture, out var width))
        {
            var points = widthMatch.Groups[2].Value.Equals("px", StringComparison.OrdinalIgnoreCase)
                ? width * 0.75
                : width;
            size = (uint)Math.Max(0, Math.Round(points * 8));
        }

        var colorMatch = Regex.Match(value, @"#([0-9a-f]{3}|[0-9a-f]{6})\b", RegexOptions.IgnoreCase);
        return (size, colorMatch.Success ? NormalizarColor(colorMatch.Value) : "000000");
    }

    private static string NormalizarColor(string value)
    {
        var color = value.Trim().TrimStart('#');
        if (color.Length == 3)
            color = string.Concat(color.Select(c => $"{c}{c}"));
        return Regex.IsMatch(color, "^[0-9a-fA-F]{6}$") ? color.ToUpperInvariant() : "000000";
    }

    private static (long Width, long Height) AjustarImagenContain(byte[] bytes, long boxWidth, long boxHeight)
    {
        var (pixelWidth, pixelHeight) = ObtenerDimensionesImagen(bytes);
        if (pixelWidth <= 0 || pixelHeight <= 0 || boxWidth <= 0 || boxHeight <= 0)
            return (boxWidth, boxHeight);

        var scale = Math.Min((double)boxWidth / pixelWidth, (double)boxHeight / pixelHeight);
        return (
            Math.Max(1, (long)Math.Round(pixelWidth * scale)),
            Math.Max(1, (long)Math.Round(pixelHeight * scale))
        );
    }

    private static (int Width, int Height) ObtenerDimensionesImagen(byte[] bytes)
    {
        if (bytes.Length >= 24
            && bytes[0] == 0x89 && bytes[1] == 0x50 && bytes[2] == 0x4E && bytes[3] == 0x47)
        {
            return (
                LeerInt32BigEndian(bytes, 16),
                LeerInt32BigEndian(bytes, 20)
            );
        }

        if (bytes.Length < 4 || bytes[0] != 0xFF || bytes[1] != 0xD8)
            return (0, 0);

        var offset = 2;
        while (offset + 8 < bytes.Length)
        {
            if (bytes[offset] != 0xFF)
            {
                offset++;
                continue;
            }

            var marker = bytes[offset + 1];
            if (marker is 0xC0 or 0xC1 or 0xC2 or 0xC3 or 0xC5 or 0xC6 or 0xC7 or 0xC9 or 0xCA or 0xCB or 0xCD or 0xCE or 0xCF)
            {
                return (
                    (bytes[offset + 7] << 8) | bytes[offset + 8],
                    (bytes[offset + 5] << 8) | bytes[offset + 6]
                );
            }

            if (marker is 0xD8 or 0xD9)
            {
                offset += 2;
                continue;
            }

            var segmentLength = (bytes[offset + 2] << 8) | bytes[offset + 3];
            if (segmentLength < 2) break;
            offset += segmentLength + 2;
        }

        return (0, 0);
    }

    private static int LeerInt32BigEndian(byte[] bytes, int offset) =>
        (bytes[offset] << 24)
        | (bytes[offset + 1] << 16)
        | (bytes[offset + 2] << 8)
        | bytes[offset + 3];

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

    private Paragraph CrearParrafoEspaciador(int heightTwips)
    {
        var para = new Paragraph();
        var pPr = new ParagraphProperties();
        pPr.Append(new SpacingBetweenLines
        {
            Before = "0",
            After = "0",
            Line = Math.Max(1, heightTwips).ToString(),
            LineRule = LineSpacingRuleValues.Exact
        });
        para.Append(pPr);
        return para;
    }

    private int CssLineSpacing(Dictionary<string, string>? css, int fontSizeHp = 0)
    {
        var effectiveFontSizeHp = fontSizeHp > 0
            ? fontSizeHp
            : css != null && css.TryGetValue("font-size", out var fontSize)
                ? PtToHalfPt(fontSize)
                : _fontSizeHp;

        if (css != null && css.TryGetValue("line-height", out var lineHeight))
        {
            var trimmed = lineHeight.Trim();
            if (trimmed.EndsWith("pt", StringComparison.OrdinalIgnoreCase) || trimmed.EndsWith("in", StringComparison.OrdinalIgnoreCase))
                return CssToTwips(trimmed);
            var m = Regex.Match(trimmed, @"([\d.]+)");
            if (m.Success && double.TryParse(m.Groups[1].Value, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out var multiplier))
                return (int)Math.Round(effectiveFontSizeHp * 10 * multiplier);
        }

        return (int)Math.Round(effectiveFontSizeHp * 10 * _lineSpacingMultiplier);
    }

    private void AplicarMargenPendienteTabla(ParagraphProperties properties, Dictionary<string, string> css)
    {
        if (_pendingTableBottomMargin <= 0) return;

        var (marginTop, _) = ObtenerMargenVertical(css);
        var (paddingTop, _) = ObtenerPaddingVertical(css);
        var spacing = properties.GetFirstChild<SpacingBetweenLines>();
        if (spacing != null)
            spacing.Before = (paddingTop + Math.Max(_pendingTableBottomMargin, marginTop)).ToString();

        _pendingTableBottomMargin = 0;
    }

    private void RegistrarParrafoBody(Paragraph paragraph, Dictionary<string, string> css)
    {
        _lastBodyParagraph = paragraph;
        (_, _lastBodyMarginBottom) = ObtenerMargenVertical(css);
        (_, _lastBodyPaddingBottom) = ObtenerPaddingVertical(css);
    }

    private void LimpiarUltimoParrafoBody()
    {
        _lastBodyParagraph = null;
        _lastBodyMarginBottom = 0;
        _lastBodyPaddingBottom = 0;
    }

    private void FlushPendingTableMargin(Body body)
    {
        if (_pendingTableBottomMargin <= 0) return;
        body.Append(CrearParrafoEspaciador(_pendingTableBottomMargin));
        _pendingTableBottomMargin = 0;
        LimpiarUltimoParrafoBody();
    }

    private static (int Top, int Bottom) ObtenerEspaciadoVertical(Dictionary<string, string> css)
    {
        var (marginTop, marginBottom) = ObtenerMargenVertical(css);
        var (paddingTop, paddingBottom) = ObtenerPaddingVertical(css);
        return (marginTop + paddingTop, marginBottom + paddingBottom);
    }

    private static (int Top, int Bottom) ObtenerMargenVertical(Dictionary<string, string> css) =>
        ObtenerEspaciadoVerticalCss(css, "margin");

    private static (int Top, int Bottom) ObtenerPaddingVertical(Dictionary<string, string> css) =>
        ObtenerEspaciadoVerticalCss(css, "padding");

    private static (int Top, int Bottom) ObtenerEspaciadoVerticalCss(
        Dictionary<string, string> css,
        string property)
    {
        var top = 0;
        var bottom = 0;

        if (css.TryGetValue(property, out var shorthand))
        {
            var parts = shorthand.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length == 1)
            {
                top = bottom = CssToTwips(parts[0]);
            }
            else if (parts.Length == 2)
            {
                top = bottom = CssToTwips(parts[0]);
            }
            else if (parts.Length >= 3)
            {
                top = CssToTwips(parts[0]);
                bottom = CssToTwips(parts[2]);
            }
        }

        if (css.TryGetValue($"{property}-top", out var explicitTop)) top = CssToTwips(explicitTop);
        if (css.TryGetValue($"{property}-bottom", out var explicitBottom)) bottom = CssToTwips(explicitBottom);

        return (top, bottom);
    }

    private static (int Top, int Right, int Bottom, int Left) ObtenerPadding(Dictionary<string, string>? css)
    {
        var top = 0;
        var right = 43;
        var bottom = 0;
        var left = 43;

        if (css != null && css.TryGetValue("padding", out var padding))
        {
            var parts = padding.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length == 1)
            {
                top = right = bottom = left = CssToTwips(parts[0]);
            }
            else if (parts.Length == 2)
            {
                top = bottom = CssToTwips(parts[0]);
                right = left = CssToTwips(parts[1]);
            }
            else if (parts.Length == 3)
            {
                top = CssToTwips(parts[0]);
                right = left = CssToTwips(parts[1]);
                bottom = CssToTwips(parts[2]);
            }
            else if (parts.Length >= 4)
            {
                top = CssToTwips(parts[0]);
                right = CssToTwips(parts[1]);
                bottom = CssToTwips(parts[2]);
                left = CssToTwips(parts[3]);
            }
        }

        if (css != null && css.TryGetValue("padding-top", out var pt)) top = CssToTwips(pt);
        if (css != null && css.TryGetValue("padding-right", out var pr)) right = CssToTwips(pr);
        if (css != null && css.TryGetValue("padding-bottom", out var pb)) bottom = CssToTwips(pb);
        if (css != null && css.TryGetValue("padding-left", out var pl)) left = CssToTwips(pl);

        return (top, right, bottom, left);
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
