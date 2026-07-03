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
    private int _pendingTableBottomMargin;
    private Paragraph? _lastBodyParagraph;
    private int _lastBodyMarginBottom;
    private int _lastBodyPaddingBottom;
    private Dictionary<string, byte[]>? _assets;
    private MainDocumentPart? _mainPart;
    private FooterPart? _defaultFooterPart;
    private FooterPart? _firstFooterPart;
    private HeaderPart? _firstHeaderPart;
    private uint _nextDrawingId = 100;
    // Max measured last-cell width across all auto-width right-aligned keyValue tables,
    // used to normalize value cells into a uniform column.
    private int _rightAlignedMaxLastCellW = 0;

    public MemoryStream GenerarDocx(JsonNode json, Dictionary<string, byte[]>? assets = null)
    {
        var ms = new MemoryStream();
        using (var doc = WordprocessingDocument.Create(ms, WordprocessingDocumentType.Document, true))
        {
            var mainPart = doc.AddMainDocumentPart();
            mainPart.Document = new Document();
            var body = new Body();

            var config = json["document"];
            var sections = json["sections"]?.AsArray();

            _assets = assets;
            _mainPart = mainPart;
            _defaultFooterPart = null;
            _firstFooterPart = null;
            _firstHeaderPart = null;
            _nextDrawingId = 100;
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
        var css = ParseCss(section["style"]?.GetValue<string>());
        if (DebeForzarSaltoPagina(section, css))
            AgregarSaltoPagina(body);

        var type = section["type"]?.GetValue<string>() ?? "";
        switch (type)
        {
            case "heading": RenderHeading(body, section); break;
            case "subtitle": RenderSubtitle(body, section); break;
            case "text": RenderText(body, section); break;
            case "inline": RenderInline(body, section); break;
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

    private static bool DebeForzarSaltoPagina(JsonNode section, Dictionary<string, string> css) =>
        (section["pageBreak"]?.GetValue<bool>() ?? false)
        || css.GetValueOrDefault("page-break-before", "") == "always"
        || css.GetValueOrDefault("break-before", "") == "page";

    private void AgregarSaltoPagina(Body body)
    {
        if (body.Elements().Any())
            body.Append(new Paragraph(new Run(new Break { Type = BreakValues.Page })));
        LimpiarUltimoParrafoBody();
        _pendingTableBottomMargin = 0;
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
        ColapsarMargenParrafoAnterior(body, pPr, css);

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
        ColapsarMargenParrafoAnterior(body, pPr, css);

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
        ColapsarMargenParrafoAnterior(body, pPr, css);
        AgregarIndentacion(pPr);
        para.Append(pPr);
        para.Append(CrearRunConSaltos(text, css));
        body.Append(para);
        RegistrarParrafoBody(para, css);
    }

    private void RenderInline(Body body, JsonNode section)
    {
        var runs = section["runs"]?.AsArray();
        if (runs == null || runs.Count == 0) return;

        var pCss = ParseCss(section["style"]?.GetValue<string>());
        var para = new Paragraph();
        var pPr = new ParagraphProperties();
        pPr.Append(new SpacingBetweenLines { After = "0", Line = CssLineSpacing(pCss).ToString(), LineRule = LineSpacingRuleValues.Exact });
        AplicarMargenPendienteTabla(pPr, pCss);
        ColapsarMargenParrafoAnterior(body, pPr, pCss);
        AgregarIndentacion(pPr);
        para.Append(pPr);

        foreach (var runNode in runs)
        {
            if (runNode == null) continue;
            var text = runNode["text"]?.GetValue<string>() ?? "";
            if (string.IsNullOrEmpty(text)) continue;
            var css = ParseCss(runNode["style"]?.GetValue<string>());
            var run = new Run();
            run.Append(CrearRunProperties(css));
            AgregarTextoAlRun(run, text, css, permitirSaltos: false);
            para.Append(run);
        }

        body.Append(para);
        RegistrarParrafoBody(para, pCss);
    }

    private void RenderKeyValue(Body body, JsonNode section)
    {
        var tblCss = ParseCss(section["style"]?.GetValue<string>());
        var rows = section["rows"]?.AsArray();
        if (rows is null || rows.Count == 0) return;

        bool fixedWidth = tblCss.ContainsKey("width");
        var tblW = fixedWidth ? ObtenerAnchoTabla(tblCss) : 0;
        bool isAutoRightAligned = !fixedWidth && ObtenerAlineacionTabla(tblCss) == TableRowAlignmentValues.Right;

        bool esOutsetKv = !tblCss.ContainsKey("border") &&
            tblCss.TryGetValue("border-top", out var kvTop) && tblCss.TryGetValue("border-left", out var kvLeft) &&
            tblCss.TryGetValue("border-bottom", out var kvBottom) && tblCss.TryGetValue("border-right", out var kvRight) &&
            EsColorClaro(ObtenerBorde(kvTop).Color) && EsColorClaro(ObtenerBorde(kvLeft).Color) &&
            !EsColorClaro(ObtenerBorde(kvBottom).Color) && !EsColorClaro(ObtenerBorde(kvRight).Color);
        var cellSpacingTwips = (!esOutsetKv && tblCss.TryGetValue("border-spacing", out var bsp))
            ? CssToTwips(bsp.Trim().Split(' ')[0]) : 0;
        var inheritTableBorderInCells = HeredarBordeTablaEnCeldas(tblCss);

        // Pre-pass: for auto-width tables, estimate total width from cell content.
        // Word auto-sizes tighter than PDF/HTML, so we compute an explicit width,
        // then switch to fixed layout so all renderers produce the same table size.
        int maxCellCount = 0;
        if (!fixedWidth)
        {
            foreach (var row in rows)
            {
                if (row is not JsonArray cells) continue;
                int rowW = 0;
                int cellCount = 0;
                foreach (var cell in cells)
                {
                    if (cell is null) continue;
                    var cellCss = ParseCss(cell["style"]?.GetValue<string>());
                    var effectiveCss = CombinarCssHeredable(tblCss, cellCss, inheritTableBorderInCells);
                    var w = CssToTwips(cellCss.GetValueOrDefault("width", ""));
                    if (w <= 0)
                        w = EstimarAnchoCeldaTwips(cell["text"]?.GetValue<string>() ?? "", effectiveCss);
                    rowW += w;
                    cellCount++;
                }
                tblW = Math.Max(tblW, rowW);
                maxCellCount = Math.Max(maxCellCount, cellCount);
            }
        }

        // Expand table width to accommodate tblCellSpacing gaps:
        // Word places (numCells + 1) gaps of spacingTwips inside the declared tblW.
        var tblWDocx = tblW + (maxCellCount > 0 ? (maxCellCount + 1) * cellSpacingTwips : 0);

        var table = CrearTabla(tblCss);
        var columnWidths = fixedWidth && tblW > 0 ? ComputarAnchosColumnasKeyValue(rows, tblW) : null;
        if (columnWidths is { Count: > 0 })
        {
            var tblGrid = new TableGrid();
            foreach (var cw in columnWidths)
                tblGrid.Append(new GridColumn { Width = cw.ToString() });
            table.Append(tblGrid);
        }

        // Upgrade auto-width table to fixed layout using the computed width.
        // Replace tblW={0,auto}+jc with tblW={n,dxa}+tblInd+tblLayout=fixed,
        // mirroring the same alignment path CrearTabla uses for declared widths.
        if (!fixedWidth && tblWDocx > 0)
        {
            var tPr = table.Elements<TableProperties>().First();
            tPr.RemoveAllChildren<TableWidth>();
            tPr.RemoveAllChildren<TableJustification>();
            tPr.RemoveAllChildren<TableIndentation>();
            var alignment = ObtenerAlineacionTabla(tblCss);
            var remaining = Math.Max(0, _contentWidth - tblWDocx);
            var offset = alignment == TableRowAlignmentValues.Center ? remaining / 2
                       : alignment == TableRowAlignmentValues.Right  ? remaining : 0;
            tPr.Append(new TableWidth { Width = tblWDocx.ToString(), Type = TableWidthUnitValues.Dxa });
            tPr.Append(new TableJustification { Val = TableRowAlignmentValues.Left });
            tPr.Append(new TableIndentation { Width = _contentIndentL + offset, Type = TableWidthUnitValues.Dxa });
            tPr.Append(new TableLayout { Type = TableLayoutValues.Fixed });
        }

        foreach (var row in rows)
        {
            if (row is not JsonArray cells) continue;

            var tr = CrearFilaTabla();
            var cellList = cells.Where(c => c is not null).ToList();
            int usedW = 0;
            int gridIdx = 0;
            var rowHeight = 0;
            foreach (var cell in cellList)
            {
                var cellCss = ParseCss(cell!["style"]?.GetValue<string>());
                rowHeight = Math.Max(rowHeight, CssToTwips(cellCss.GetValueOrDefault("height", "")));
            }
            AplicarAltoFila(tr, rowHeight);

            for (int i = 0; i < cellList.Count; i++)
            {
                var cell = cellList[i]!;
                var text = cell["text"]?.GetValue<string>() ?? "";
                var cellCss = ParseCss(cell["style"]?.GetValue<string>());
                var effectiveCss = CombinarCssHeredable(tblCss, cellCss, inheritTableBorderInCells);
                var imgBytes = ResolveAssetBytes(cell["image"]?.GetValue<string>());
                var colspan = Math.Max(1, cell["colspan"]?.GetValue<int>() ?? 1);

                int cellW;
                bool isAutoCell = CssToTwips(cellCss.GetValueOrDefault("width", "")) <= 0;
                if (columnWidths != null && gridIdx < columnWidths.Count)
                    cellW = columnWidths.Skip(gridIdx).Take(colspan).Sum();
                else if (i == cellList.Count - 1 && tblW > 0)
                    cellW = Math.Max(0, tblW - usedW);
                else
                {
                    cellW = isAutoCell ? 0 : CssToTwips(cellCss["width"]);
                    if (cellW <= 0)
                        cellW = EstimarAnchoCeldaTwips(text, effectiveCss);
                }

                // Auto-width cells in right-aligned tables flush their text to the
                // right so all value cells share the same visual right boundary,
                // regardless of how wide each individual cell is.
                if (isAutoRightAligned && isAutoCell)
                    effectiveCss["text-align"] = "right";

                tr.Append(CrearCeldaTexto(text, cellW, effectiveCss, colspan: colspan, imageBytes: imgBytes));
                usedW += cellW;
                gridIdx += colspan;
            }

            table.Append(tr);
        }

        AgregarTablaConMargen(body, table, tblCss);
    }

    private static List<int> ComputarAnchosColumnasKeyValue(JsonArray rows, int tableWidth)
    {
        var columnCount = rows
            .OfType<JsonArray>()
            .Select(row => row.Sum(cell => cell is null ? 0 : Math.Max(1, cell["colspan"]?.GetValue<int>() ?? 1)))
            .DefaultIfEmpty(0)
            .Max();
        if (columnCount <= 0 || tableWidth <= 0) return [];

        var widths = new int[columnCount];
        foreach (var row in rows.OfType<JsonArray>())
        {
            var cells = row.Where(cell => cell is not null).ToList();
            var gridIdx = 0;
            for (var i = 0; i < cells.Count && gridIdx < columnCount; i++)
            {
                var cellCss = ParseCss(cells[i]!["style"]?.GetValue<string>());
                var explicitWidth = CssToTwips(cellCss.GetValueOrDefault("width", ""));
                var colspan = Math.Max(1, cells[i]!["colspan"]?.GetValue<int>() ?? 1);
                if (explicitWidth > 0 && colspan == 1)
                    widths[gridIdx] = Math.Max(widths[gridIdx], explicitWidth);
                gridIdx += colspan;
            }
        }

        var fixedTotal = widths.Sum();
        if (fixedTotal > tableWidth)
        {
            var scale = (double)tableWidth / fixedTotal;
            for (var i = 0; i < widths.Length; i++)
                widths[i] = (int)Math.Round(widths[i] * scale);
            return [.. widths];
        }

        var autoIndexes = widths
            .Select((width, index) => (width, index))
            .Where(item => item.width <= 0)
            .Select(item => item.index)
            .ToList();

        if (autoIndexes.Count == 0)
        {
            var missing = tableWidth - fixedTotal;
            if (missing > 0)
                widths[^1] += missing;
            return [.. widths];
        }

        var autoWidth = Math.Max(0, tableWidth - fixedTotal) / autoIndexes.Count;
        foreach (var index in autoIndexes)
            widths[index] = autoWidth;

        var roundingMissing = tableWidth - widths.Sum();
        if (roundingMissing != 0)
            widths[autoIndexes[^1]] += roundingMissing;

        return [.. widths];
    }

    private int EstimarAnchoCeldaTwips(string text, Dictionary<string, string> css)
    {
        var hp = css.TryGetValue("font-size", out var fs) ? PtToHalfPt(fs) : _fontSizeHp;
        var sizePt = hp / 2.0;
        var family = ObtenerFamiliaFuente(css);
        var bold = css.GetValueOrDefault("font-weight") is "bold" or "700";
        var italic = css.GetValueOrDefault("font-style") is "italic" or "oblique";
        var textPts = DocxFontMeasurer.MeasureString(text, family, sizePt, bold, italic);
        var (_, padR, _, padL) = ObtenerPadding(css);
        return (int)(textPts * 20) + padL + padR;
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
            var titlePPr = CrearParagraphProps(titleCss);
            ColapsarMargenParrafoAnterior(body, titlePPr, titleCss);
            pTitle.Append(titlePPr);
            pTitle.Append(CrearRun(title, titleCss));
            body.Append(pTitle);
            RegistrarParrafoBody(pTitle, titleCss);

            if (!string.IsNullOrEmpty(content))
            {
                var pContent = new Paragraph();
                var pPr = new ParagraphProperties();
                pPr.Append(new SpacingBetweenLines { After = "0", Line = CssLineSpacing(contentCss).ToString(), LineRule = LineSpacingRuleValues.Exact });
                AplicarMargenPendienteTabla(pPr, contentCss);
                ColapsarMargenParrafoAnterior(body, pPr, contentCss);
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
        FlushPendingTableMargin(body);
        body.Append(CrearParrafoEspaciador(CssToTwips(section["height"]?.GetValue<string>() ?? "0.3in")));
        LimpiarUltimoParrafoBody();
    }

    private byte[]? ResolveAssetBytes(string? valor)
    {
        if (string.IsNullOrWhiteSpace(valor) || _assets is null) return null;
        if (valor.StartsWith("{") && valor.EndsWith("}"))
            return _assets.TryGetValue(valor[1..^1], out var b) ? b : null;
        return null;
    }

    // ==================== HEADER / FOOTER / PAGE ====================

    private void AgregarHeaderLogo(MainDocumentPart mainPart, JsonNode? config)
    {
        var logoBytes = ResolveAssetBytes(config?["header"]?["logo"]?.GetValue<string>());
        var watermarkBytes = ResolveAssetBytes(config?["watermark"]?["image"]?.GetValue<string>());

        var fpWatermarkBytes = ResolveAssetBytes(config?["firstPageWatermark"]?["image"]?.GetValue<string>());
        var hasLogo = logoBytes is not null && logoBytes.Length > 0;
        var hasWatermark = watermarkBytes is not null && watermarkBytes.Length > 0;
        var hasFirstPageWatermark = fpWatermarkBytes is not null && fpWatermarkBytes.Length > 0;
        if (!hasLogo && !hasWatermark && !hasFirstPageWatermark) return;

        HeaderPart? headerPart = null;
        Header? header = null;

        if (hasWatermark || hasLogo)
        {
            headerPart = mainPart.AddNewPart<HeaderPart>();
            header = new Header();
        }

        if (headerPart != null && header != null)
        {
            if (hasLogo)
                AgregarLogoAlHeader(headerPart, header, config, logoBytes!);

            AgregarMarcaAguaAlHeader(headerPart, header, config, watermarkBytes);

            headerPart.Header = header;
            headerPart.Header.Save();
        }

        if (hasFirstPageWatermark)
        {
            _firstHeaderPart = mainPart.AddNewPart<HeaderPart>();
            var fpHeader = new Header();
            if (hasLogo)
                AgregarLogoAlHeader(_firstHeaderPart, fpHeader, config, logoBytes!);
            AgregarMarcaAguaAlHeader(_firstHeaderPart, fpHeader, config, fpWatermarkBytes, firstPage: true);
            _firstHeaderPart.Header = fpHeader;
            _firstHeaderPart.Header.Save();
        }
    }

    private void AgregarLogoAlHeader(HeaderPart headerPart, Header header, JsonNode? config, byte[] logoBytes)
    {
        var logoBoxW = CssToEmu(config?["header"]?["logoWidth"]?.GetValue<string>() ?? "1.3in");
        var logoBoxH = CssToEmu(config?["header"]?["logoHeight"]?.GetValue<string>() ?? "0.55in");
        var (logoW, logoH) = AjustarImagenContain(logoBytes, logoBoxW, logoBoxH);
        var align = config?["header"]?["align"]?.GetValue<string>() ?? "center";

        var imagePart = logoBytes.Length >= 2 && logoBytes[0] == 0xFF && logoBytes[1] == 0xD8
            ? headerPart.AddImagePart(ImagePartType.Jpeg)
            : headerPart.AddImagePart(ImagePartType.Png);
        using (var imgStream = new MemoryStream(logoBytes))
            imagePart.FeedData(imgStream);

        var relationshipId = headerPart.GetIdOfPart(imagePart);

        var drawing = new Drawing(
            new DW.Inline(
                new DW.Extent { Cx = logoW, Cy = logoH },
                new DW.EffectExtent { LeftEdge = 0, TopEdge = 0, RightEdge = 0, BottomEdge = 0 },
                new DW.DocProperties { Id = _nextDrawingId++, Name = "Logo" },
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
        var headerMarginTop = CssToTwips(config?["header"]?["marginTop"]?.GetValue<string>() ?? "0");
        var verticalPadding = Math.Max(0, (int)((logoBoxH - logoH) / 635 / 2));
        pPr.Append(new SpacingBetweenLines
        {
            Before = (headerMarginTop + verticalPadding).ToString(),
            After = (gapAfter + verticalPadding).ToString(),
            Line = (1 * 240).ToString(),
            LineRule = LineSpacingRuleValues.Auto
        });
        para.Append(pPr);

        var run = new Run();
        run.Append(drawing);
        para.Append(run);

        header.Append(para);
    }

    private void AgregarMarcaAguaAlHeader(HeaderPart headerPart, Header header, JsonNode? config, byte[]? watermarkBytes, bool firstPage = false)
    {
        if (watermarkBytes is null || watermarkBytes.Length == 0) return;

        var wmNode = firstPage ? config?["firstPageWatermark"] : config?["watermark"];
        if (wmNode is null) return;

        var wmWidthPt = CssToPt(wmNode["width"]?.GetValue<string>() ?? "0");
        var wmHeightPt = CssToPt(wmNode["height"]?.GetValue<string>() ?? "0");
        if (wmWidthPt <= 0 || wmHeightPt <= 0) return;

        var pageW = CssToPt(config?["pageSize"]?["width"]?.GetValue<string>() ?? "8.5in");
        var pageH = CssToPt(config?["pageSize"]?["height"]?.GetValue<string>() ?? "11in");

        var position = wmNode["position"]?.GetValue<string>() ?? "center center";
        var parts = position.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        var hPos = parts.Length > 0 ? parts[0] : "center";
        var vPos = parts.Length > 1 ? parts[1] : "center";

        var marginLeft = hPos switch
        {
            "left" => 0.0,
            "right" => pageW - wmWidthPt,
            _ => (pageW - wmWidthPt) / 2
        };
        var marginTop = vPos switch
        {
            "top" => 0.0,
            "bottom" => pageH - wmHeightPt,
            _ => (pageH - wmHeightPt) / 2
        };

        var imagePart = watermarkBytes.Length >= 2 && watermarkBytes[0] == 0xFF && watermarkBytes[1] == 0xD8
            ? headerPart.AddImagePart(ImagePartType.Jpeg)
            : headerPart.AddImagePart(ImagePartType.Png);
        using (var imgStream = new MemoryStream(watermarkBytes))
            imagePart.FeedData(imgStream);

        var relId = headerPart.GetIdOfPart(imagePart);
        var (wmW, wmH) = AjustarImagenContain(watermarkBytes, CssToEmu(wmNode["width"]?.GetValue<string>() ?? "0"), CssToEmu(wmNode["height"]?.GetValue<string>() ?? "0"));
        var marginLeftEmu = (long)(marginLeft * 12700);
        var marginTopEmu = (long)(marginTop * 12700);

        var drawing = new Drawing(
            new DW.Anchor(
                new DW.SimplePosition { X = 0, Y = 0 },
                new DW.HorizontalPosition(
                    new DW.PositionOffset(marginLeftEmu.ToString())
                ) { RelativeFrom = DW.HorizontalRelativePositionValues.Page },
                new DW.VerticalPosition(
                    new DW.PositionOffset(marginTopEmu.ToString())
                ) { RelativeFrom = DW.VerticalRelativePositionValues.Page },
                new DW.Extent { Cx = wmW, Cy = wmH },
                new DW.EffectExtent { LeftEdge = 0, TopEdge = 0, RightEdge = 0, BottomEdge = 0 },
                new DW.WrapNone(),
                new DW.DocProperties { Id = 2, Name = "Watermark" },
                new DW.NonVisualGraphicFrameDrawingProperties(
                    new A.GraphicFrameLocks { NoChangeAspect = true }),
                new A.Graphic(
                    new A.GraphicData(
                        new PIC.Picture(
                            new PIC.NonVisualPictureProperties(
                                new PIC.NonVisualDrawingProperties { Id = 0, Name = "watermark.png" },
                                new PIC.NonVisualPictureDrawingProperties()),
                            new PIC.BlipFill(
                                new A.Blip { Embed = relId },
                                new A.Stretch(new A.FillRectangle())),
                            new PIC.ShapeProperties(
                                new A.Transform2D(
                                    new A.Offset { X = 0, Y = 0 },
                                    new A.Extents { Cx = wmW, Cy = wmH }),
                                new A.PresetGeometry(new A.AdjustValueList()) { Preset = A.ShapeTypeValues.Rectangle }))
                    ) { Uri = "http://schemas.openxmlformats.org/drawingml/2006/picture" })
            )
            {
                DistanceFromTop = 0, DistanceFromBottom = 0,
                DistanceFromLeft = 0, DistanceFromRight = 0,
                SimplePos = false, RelativeHeight = 0,
                BehindDoc = true, Locked = false,
                LayoutInCell = true, AllowOverlap = true
            });

        var wmPara = new Paragraph();
        var wmRun = new Run();
        wmRun.Append(drawing);
        wmPara.Append(wmRun);
        header.Append(wmPara);
    }

    private static double CssToPt(string value)
    {
        if (string.IsNullOrEmpty(value)) return 0;
        var m = Regex.Match(value, @"([\d.]+)\s*(in|pt|)");
        if (!m.Success || !double.TryParse(m.Groups[1].Value, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out var num))
            return 0;
        return m.Groups[2].Value switch
        {
            "in" => num * 72,
            "pt" => num,
            _ => num * 72
        };
    }

    private void AgregarFooter(MainDocumentPart mainPart, JsonNode? config)
    {
        var footerText = config?["footer"]?["text"]?.GetValue<string>();
        var footerPart = mainPart.AddNewPart<FooterPart>();
        _defaultFooterPart = footerPart;
        var footer = new Footer();
        var footerFontSizeHp = PtToHalfPt(config?["footer"]?["fontSize"]?.GetValue<string>() ?? "7pt");
        var footerFontSize = footerFontSizeHp.ToString();
        var footerLineHeight = (footerFontSizeHp * 10).ToString();
        var footerAlign = config?["footer"]?["align"]?.GetValue<string>() ?? "left";
        var pageAlign = config?["footer"]?["pageAlign"]?.GetValue<string>() ?? "below";
        var fiL = CssToTwips(config?["footerIndent"]?["left"]?.GetValue<string>() ?? "0");
        var fiR = CssToTwips(config?["footerIndent"]?["right"]?.GetValue<string>() ?? "0");
        var gapBefore = CssToTwips(config?["footer"]?["gapBefore"]?.GetValue<string>() ?? "0");
        var footerMBottom = CssToTwips(config?["footer"]?["marginBottom"]?.GetValue<string>() ?? "0");
        var showPageNumber = config?["footer"]?["showPageNumber"]?.GetValue<bool>() ?? true;
        var footerPageW = CssToTwips(config?["pageSize"]?["width"]?.GetValue<string>());
        var footerMl = CssToTwips(config?["margins"]?["left"]?.GetValue<string>());
        var footerMr = CssToTwips(config?["margins"]?["right"]?.GetValue<string>());
        var footerExtend = CssToTwips(config?["footer"]?["footerExtend"]?.GetValue<string>() ?? "0");
        var footerTableWidth = footerPageW - footerMl - footerMr + footerExtend * 2;
        var footerTableInd = footerExtend > 0 ? -footerExtend : 0;

        // Generic rows/cells table layout
        var footerLayout = config?["footer"]?["layout"]?.GetValue<string>();
        var footerJsonRows = config?["footer"]?["rows"]?.AsArray();
        if (footerLayout == "table" && footerJsonRows != null)
        {
            if (gapBefore > 0)
                footer.Append(new Paragraph(new ParagraphProperties(
                    new SpacingBetweenLines { Before = gapBefore.ToString(), After = "0", Line = "1", LineRule = LineSpacingRuleValues.Exact })));
            footer.Append(ConstruirTablaFooterGenerico(footerJsonRows, footerTableWidth, footerTableInd, config?["footer"], footerPart));
            footer.Append(CrearParrafoCero());
            footerPart.Footer = footer;
            footerPart.Footer.Save();

            // First-page footer
            var fpConfig = config?["firstPageFooter"];
            var fpRows = fpConfig?["rows"]?.AsArray();
            if (fpConfig != null && fpRows != null)
            {
                var fpExtend = CssToTwips(fpConfig["footerExtend"]?.GetValue<string>() ?? "0");
                var fpTableWidth = footerPageW - footerMl - footerMr + fpExtend * 2;
                var fpTableInd = fpExtend > 0 ? -fpExtend : 0;
                var fpGapBefore = CssToTwips(fpConfig["gapBefore"]?.GetValue<string>() ?? "0");
                var fpFooterPart = mainPart.AddNewPart<FooterPart>();
                _firstFooterPart = fpFooterPart;
                var fpFooter = new Footer();
                if (fpGapBefore > 0)
                    fpFooter.Append(new Paragraph(new ParagraphProperties(
                        new SpacingBetweenLines { Before = fpGapBefore.ToString(), After = "0", Line = "1", LineRule = LineSpacingRuleValues.Exact })));
                fpFooter.Append(ConstruirTablaFooterGenerico(fpRows, fpTableWidth, fpTableInd, fpConfig, fpFooterPart));
                var defaultFooterHeight = gapBefore + EstimarAltoTablaFooter(footerJsonRows, config?["footer"]);
                var firstFooterHeight = fpGapBefore + EstimarAltoTablaFooter(fpRows, fpConfig);
                var missingFooterHeight = defaultFooterHeight - firstFooterHeight;
                if (missingFooterHeight > 0)
                    fpFooter.Append(CrearParrafoAlto(missingFooterHeight));
                fpFooter.Append(CrearParrafoCero());
                fpFooterPart.Footer = fpFooter;
                fpFooterPart.Footer.Save();
            }

            return;
        }

        var footerTable = new Table();
        footerTable.Append(new TableProperties(
            new TableWidth { Width = footerTableWidth.ToString(), Type = TableWidthUnitValues.Dxa },
            new TableIndentation { Width = footerTableInd, Type = TableWidthUnitValues.Dxa },
            new TableLayout { Type = TableLayoutValues.Fixed }));

        var footerBoxHeight = CssToTwips(config?["margins"]?["bottom"]?.GetValue<string>()) - footerMBottom - gapBefore;

        var footerRow = new TableRow();
        footerRow.Append(new TableRowProperties(
            new TableRowHeight { Val = (uint)footerBoxHeight, HeightType = HeightRuleValues.Exact }));

        if (pageAlign == "left" || pageAlign == "right")
        {
            // Two-cell row: page number on one side, text on the other
            var pageBgColorVal = config?["footer"]?["pageBgColor"]?.GetValue<string>();
            var textColorVal   = config?["footer"]?["textColor"]?.GetValue<string>();
            var textBold       = config?["footer"]?["textBold"]?.GetValue<bool>() ?? false;
            var pageTotal      = config?["footer"]?["pageTotal"]?.GetValue<bool>() ?? false;
            var pageColW       = CssToTwips(config?["footer"]?["pageColWidth"]?.GetValue<string>() ?? "0");
            if (pageColW == 0) pageColW = footerTableWidth / 2;
            var textColW       = footerTableWidth - pageColW;

            TableCell BuildCell(int width, JustificationValues justify, string? bgColor, string marginL, string marginR)
            {
                var tcPr = new TableCellProperties(
                    new TableCellWidth { Width = width.ToString(), Type = TableWidthUnitValues.Dxa },
                    new TableCellMargin(
                        new TopMargin { Width = "0", Type = TableWidthUnitValues.Dxa },
                        new BottomMargin { Width = "0", Type = TableWidthUnitValues.Dxa },
                        new LeftMargin { Width = marginL, Type = TableWidthUnitValues.Dxa },
                        new RightMargin { Width = marginR, Type = TableWidthUnitValues.Dxa }),
                    new TableCellVerticalAlignment { Val = TableVerticalAlignmentValues.Center });
                if (!string.IsNullOrEmpty(bgColor))
                    tcPr.Append(new Shading { Val = ShadingPatternValues.Clear, Color = "auto", Fill = NormalizarColor(bgColor) });
                var cell = new TableCell();
                cell.Append(tcPr);
                return cell;
            }

            var pageFontSizeHp = PtToHalfPt(config?["footer"]?["pageFontSize"]?.GetValue<string>() ?? config?["footer"]?["fontSize"]?.GetValue<string>() ?? "7pt");
            var pageFontSizeStr = pageFontSizeHp.ToString();
            var pageLineHeight = (pageFontSizeHp * 10).ToString();
            var pageColorVal = config?["footer"]?["pageColor"]?.GetValue<string>();
            var pageLabel = config?["footer"]?["pageLabel"]?.GetValue<string>() ?? "Page";

            RunProperties BuildPageRunProps()
            {
                var rp = new RunProperties(new FontSize { Val = pageFontSizeStr }, new RunFonts { Ascii = _fontFamily, HighAnsi = _fontFamily });
                if (!string.IsNullOrEmpty(pageColorVal))
                    rp.Append(new Color { Val = NormalizarColor(pageColorVal) });
                return rp;
            }

            // Page number cell
            var pageJustify = pageAlign == "left" ? JustificationValues.Left : JustificationValues.Right;
            var pageCell = BuildCell((int)pageColW, pageJustify, pageBgColorVal, fiL.ToString(), "0");
            if (showPageNumber)
            {
                var paraPage = new Paragraph();
                var pPrPage = new ParagraphProperties();
                pPrPage.Append(new Justification { Val = JustificationValues.Center });
                pPrPage.Append(new SpacingBetweenLines { Before = "0", After = "0", Line = pageLineHeight, LineRule = LineSpacingRuleValues.Exact });
                paraPage.Append(pPrPage);
                var runPage = new Run();
                runPage.Append(BuildPageRunProps());
                runPage.Append(new Text(pageLabel + " ") { Space = SpaceProcessingModeValues.Preserve });
                paraPage.Append(runPage);
                var runField = new Run();
                runField.Append(BuildPageRunProps());
                runField.Append(new FieldChar { FieldCharType = FieldCharValues.Begin });
                paraPage.Append(runField);
                var runCode = new Run();
                runCode.Append(BuildPageRunProps());
                runCode.Append(new FieldCode(" PAGE ") { Space = SpaceProcessingModeValues.Preserve });
                paraPage.Append(runCode);
                var runEnd = new Run();
                runEnd.Append(BuildPageRunProps());
                runEnd.Append(new FieldChar { FieldCharType = FieldCharValues.End });
                paraPage.Append(runEnd);

                if (pageTotal)
                {
                    var runOf = new Run();
                    runOf.Append(BuildPageRunProps());
                    runOf.Append(new Text(" of ") { Space = SpaceProcessingModeValues.Preserve });
                    paraPage.Append(runOf);

                    var runTotalField = new Run();
                    runTotalField.Append(BuildPageRunProps());
                    runTotalField.Append(new FieldChar { FieldCharType = FieldCharValues.Begin });
                    paraPage.Append(runTotalField);

                    var runTotalCode = new Run();
                    runTotalCode.Append(BuildPageRunProps());
                    runTotalCode.Append(new FieldCode(" NUMPAGES ") { Space = SpaceProcessingModeValues.Preserve });
                    paraPage.Append(runTotalCode);

                    var runTotalEnd = new Run();
                    runTotalEnd.Append(BuildPageRunProps());
                    runTotalEnd.Append(new FieldChar { FieldCharType = FieldCharValues.End });
                    paraPage.Append(runTotalEnd);
                }

                pageCell.Append(paraPage);
            }
            else
            {
                pageCell.Append(new Paragraph());
            }

            // Text cell
            var textJustify = pageAlign == "left" ? JustificationValues.Left : JustificationValues.Right;
            var textCell = BuildCell((int)textColW, textJustify, null, "0", fiR.ToString());
            if (!string.IsNullOrEmpty(footerText))
            {
                var para = new Paragraph();
                var pPr = new ParagraphProperties();
                pPr.Append(new Justification { Val = textJustify });
                pPr.Append(new SpacingBetweenLines { Before = "0", After = "0", Line = footerLineHeight, LineRule = LineSpacingRuleValues.Exact });
                para.Append(pPr);
                var runProps = new RunProperties(new FontSize { Val = footerFontSize }, new RunFonts { Ascii = _fontFamily, HighAnsi = _fontFamily });
                if (textBold) runProps.Append(new Bold());
                if (!string.IsNullOrEmpty(textColorVal)) runProps.Append(new Color { Val = NormalizarColor(textColorVal) });
                var run = new Run();
                run.Append(runProps);
                var footerLines = footerText.Replace("\r\n", "\n").Replace('\r', '\n').Split('\n');
                for (var i = 0; i < footerLines.Length; i++)
                {
                    if (i > 0) run.Append(new Break());
                    run.Append(new Text(footerLines[i]) { Space = SpaceProcessingModeValues.Preserve });
                }
                para.Append(run);
                textCell.Append(para);
            }
            else
            {
                textCell.Append(new Paragraph());
            }

            if (pageAlign == "left")
            {
                footerRow.Append(pageCell);
                footerRow.Append(textCell);
            }
            else
            {
                footerRow.Append(textCell);
                footerRow.Append(pageCell);
            }
        }
        else
        {
            // Single cell: text above, page number below
            var footerCell = new TableCell();
            footerCell.Append(new TableCellProperties(
                new TableCellWidth { Width = "5000", Type = TableWidthUnitValues.Pct },
                new TableCellMargin(
                    new TopMargin { Width = "0", Type = TableWidthUnitValues.Dxa },
                    new BottomMargin { Width = "0", Type = TableWidthUnitValues.Dxa },
                    new LeftMargin { Width = fiL.ToString(), Type = TableWidthUnitValues.Dxa },
                    new RightMargin { Width = fiR.ToString(), Type = TableWidthUnitValues.Dxa }),
                new TableCellVerticalAlignment { Val = TableVerticalAlignmentValues.Top }));

            if (!string.IsNullOrEmpty(footerText))
            {
                var para = new Paragraph();
                var pPr = new ParagraphProperties();
                pPr.Append(new Justification { Val = MapAlign(footerAlign) });
                pPr.Append(new SpacingBetweenLines { Before = "0", After = "0", Line = footerLineHeight, LineRule = LineSpacingRuleValues.Exact });
                para.Append(pPr);
                var run = new Run();
                var footerRunProps = new RunProperties(new FontSize { Val = footerFontSize }, new RunFonts { Ascii = _fontFamily, HighAnsi = _fontFamily });
                var fwVal = config?["footer"]?["fontWeight"]?.GetValue<string>();
                var fiVal = config?["footer"]?["fontStyle"]?.GetValue<string>();
                var ftColor = config?["footer"]?["textColor"]?.GetValue<string>();
                if (fwVal is "bold" or "700" || (int.TryParse(fwVal, out var fwn) && fwn >= 600)) footerRunProps.Append(new Bold());
                if (fiVal is "italic" or "oblique") footerRunProps.Append(new Italic());
                if (!string.IsNullOrEmpty(ftColor)) footerRunProps.Append(new Color { Val = NormalizarColor(ftColor) });
                run.Append(footerRunProps);
                var footerLines = footerText.Replace("\r\n", "\n").Replace('\r', '\n').Split('\n');
                for (var i = 0; i < footerLines.Length; i++)
                {
                    if (i > 0) run.Append(new Break());
                    run.Append(new Text(footerLines[i]) { Space = SpaceProcessingModeValues.Preserve });
                }
                para.Append(run);
                footerCell.Append(para);
            }

            if (showPageNumber)
            {
                var pageFontSizeHp = PtToHalfPt(config?["footer"]?["pageFontSize"]?.GetValue<string>() ?? config?["footer"]?["fontSize"]?.GetValue<string>() ?? "7pt");
                var pageFontSizeStr = pageFontSizeHp.ToString();
                var pageLineHeight = (pageFontSizeHp * 10).ToString();
                var pageColorVal = config?["footer"]?["pageColor"]?.GetValue<string>();

                RunProperties BuildPageRunProps()
                {
                    var rp = new RunProperties(new FontSize { Val = pageFontSizeStr }, new RunFonts { Ascii = _fontFamily, HighAnsi = _fontFamily });
                    if (!string.IsNullOrEmpty(pageColorVal))
                        rp.Append(new Color { Val = NormalizarColor(pageColorVal) });
                    return rp;
                }

                var paraPage = new Paragraph();
                var pPrPage = new ParagraphProperties();
                pPrPage.Append(new Justification { Val = MapAlign(footerAlign) });
                var pageGapBefore = CssToTwips(config?["footer"]?["pageGapBefore"]?.GetValue<string>() ?? "0");
                pPrPage.Append(new SpacingBetweenLines { Before = pageGapBefore.ToString(), After = "0", Line = pageLineHeight, LineRule = LineSpacingRuleValues.Exact });
                paraPage.Append(pPrPage);

                var pageLabel = config?["footer"]?["pageLabel"]?.GetValue<string>() ?? "Page";
                var runPage = new Run();
                runPage.Append(BuildPageRunProps());
                runPage.Append(new Text(pageLabel + " ") { Space = SpaceProcessingModeValues.Preserve });
                paraPage.Append(runPage);

                var runField = new Run();
                runField.Append(BuildPageRunProps());
                runField.Append(new FieldChar { FieldCharType = FieldCharValues.Begin });
                paraPage.Append(runField);

                var runCode = new Run();
                runCode.Append(BuildPageRunProps());
                runCode.Append(new FieldCode(" PAGE ") { Space = SpaceProcessingModeValues.Preserve });
                paraPage.Append(runCode);

                var runEnd = new Run();
                runEnd.Append(BuildPageRunProps());
                runEnd.Append(new FieldChar { FieldCharType = FieldCharValues.End });
                paraPage.Append(runEnd);

                footerCell.Append(paraPage);
            }

            if (!footerCell.Elements<Paragraph>().Any())
                footerCell.Append(new Paragraph());

            footerRow.Append(footerCell);
        }

        footerTable.Append(footerRow);
        footer.Append(footerTable);

        footerPart.Footer = footer;
        footerPart.Footer.Save();
    }

    // ==================== GENERIC FOOTER TABLE ====================

    private Table ConstruirTablaFooterGenerico(JsonArray jsonRows, int tableWidth, int tableInd, JsonNode? footerConfig, FooterPart footerPart, bool aplicarContainerStyle = true)
    {
        var defaultFontSizeHp = PtToHalfPt(footerConfig?["fontSize"]?.GetValue<string>() ?? "11pt");
        var pageLabel      = footerConfig?["pageLabel"]?.GetValue<string>() ?? "Page";
        var pageTotalLabel = footerConfig?["pageTotalLabel"]?.GetValue<string>() ?? "of";
        var pageTotal      = footerConfig?["pageTotal"]?.GetValue<bool>() ?? false;
        var pageBgColor    = footerConfig?["pageBgColor"]?.GetValue<string>();
        var pageColor      = footerConfig?["pageColor"]?.GetValue<string>();
        var pageColWidth   = CssToTwips(footerConfig?["pageColWidth"]?.GetValue<string>() ?? "0");

        var gridColWidths = ComputarColumnasFooter(jsonRows, tableWidth, pageColWidth);

        var table = new Table();
        var tableProperties = new TableProperties(
            new TableWidth { Width = tableWidth.ToString(), Type = TableWidthUnitValues.Dxa },
            new TableIndentation { Width = tableInd, Type = TableWidthUnitValues.Dxa },
            new TableLayout { Type = TableLayoutValues.Fixed });
        if (aplicarContainerStyle)
            AplicarContainerStyleFooter(tableProperties, footerConfig);
        table.Append(tableProperties);

        var tblGrid = new TableGrid();
        foreach (var cw in gridColWidths) tblGrid.Append(new GridColumn { Width = cw.ToString() });
        table.Append(tblGrid);

        foreach (var jsonRow in jsonRows)
        {
            if (jsonRow is not JsonObject rowObj) continue;
            var cells = rowObj["cells"]?.AsArray();
            if (cells is null || cells.Count == 0) continue;

            var tableRow = new TableRow();
            int gridIdx = 0;

            foreach (var cellNode in cells)
            {
                if (cellNode is null) continue;
                var css     = ParseCss(cellNode["style"]?.GetValue<string>());
                var cls     = cellNode["class"]?.GetValue<string>() ?? "";
                var colspan = cellNode["colspan"]?.GetValue<int>() ?? 1;
                var text    = cellNode["text"]?.GetValue<string>() ?? "";
                var imgKey  = cellNode["image"]?.GetValue<string>() ?? "";

                var cellWidth = 0;
                for (int k = gridIdx; k < Math.Min(gridIdx + colspan, gridColWidths.Count); k++)
                    cellWidth += gridColWidths[k];
                gridIdx += colspan;

                TableCell tc;
                if (cls == "sr-pie-pagnum")
                    tc = FooterCeldaPageNum(cellWidth, css, pageLabel, pageTotalLabel, pageTotal, pageBgColor, pageColor, defaultFontSizeHp, colspan);
                else if (cellNode["rows"] is JsonArray nestedRows)
                    tc = FooterCeldaConTablaInterna(nestedRows, cellWidth, css, colspan, defaultFontSizeHp, footerConfig, footerPart);
                else if (!string.IsNullOrEmpty(imgKey))
                    tc = FooterCeldaImagen(ResolveAssetBytes(imgKey), cellWidth, css, colspan,
                            cellNode["imageWidth"]?.GetValue<string>() ?? "",
                            cellNode["imageHeight"]?.GetValue<string>() ?? "", footerPart);
                else
                    tc = FooterCeldaTexto(text, cellWidth, css, colspan, defaultFontSizeHp);

                tableRow.Append(tc);
            }

            table.Append(tableRow);
        }

        return table;
    }

    private Table ConstruirCajaTablaFooter(JsonArray jsonRows, int tableWidth, int tableInd, int boxHeight, int paddingTop, JsonNode? footerConfig, FooterPart footerPart)
    {
        var outerTable = new Table();
        outerTable.Append(new TableProperties(
            new TableWidth { Width = tableWidth.ToString(), Type = TableWidthUnitValues.Dxa },
            new TableIndentation { Width = tableInd, Type = TableWidthUnitValues.Dxa },
            new TableLayout { Type = TableLayoutValues.Fixed }));
        outerTable.Append(new TableGrid(new GridColumn { Width = tableWidth.ToString() }));

        var outerRow = new TableRow();
        if (boxHeight > 0)
        {
            outerRow.Append(new TableRowProperties(
                new TableRowHeight { Val = (uint)boxHeight, HeightType = HeightRuleValues.Exact }));
        }

        var outerCell = new TableCell();
        outerCell.Append(new TableCellProperties(
            new TableCellWidth { Width = tableWidth.ToString(), Type = TableWidthUnitValues.Dxa },
            new TableCellMargin(
                new TopMargin { Width = paddingTop.ToString(), Type = TableWidthUnitValues.Dxa },
                new BottomMargin { Width = "0", Type = TableWidthUnitValues.Dxa },
                new LeftMargin { Width = "0", Type = TableWidthUnitValues.Dxa },
                new RightMargin { Width = "0", Type = TableWidthUnitValues.Dxa }),
            new TableCellVerticalAlignment { Val = TableVerticalAlignmentValues.Center }));

        outerCell.Append(ConstruirTablaFooterGenerico(jsonRows, tableWidth, 0, footerConfig, footerPart));
        outerCell.Append(CrearParrafoCero());
        outerRow.Append(outerCell);
        outerTable.Append(outerRow);
        return outerTable;
    }

    private List<int> ComputarColumnasFooter(JsonArray rows, int tableWidth, int pageColWidth)
    {
        JsonArray? refRow = null;
        int bestGridCols = 0;
        foreach (var row in rows)
        {
            if (row is not JsonObject ro) continue;
            var cells = ro["cells"]?.AsArray();
            if (cells == null) continue;
            int count = cells.Sum(c => c?["colspan"]?.GetValue<int>() ?? 1);
            if (count > bestGridCols) { bestGridCols = count; refRow = cells; }
        }

        if (refRow == null || bestGridCols == 0) return [tableWidth];

        var widths = new int[bestGridCols];
        int usedFixed = 0;
        var autoIndices = new List<int>();
        int gi = 0;

        foreach (var cellNode in refRow)
        {
            if (cellNode == null) continue;
            var colspan = cellNode["colspan"]?.GetValue<int>() ?? 1;
            var cls = cellNode["class"]?.GetValue<string>() ?? "";
            var css = ParseCss(cellNode["style"]?.GetValue<string>());

            int w = cls == "sr-pie-pagnum" && pageColWidth > 0
                ? pageColWidth
                : CssToTwips(css.GetValueOrDefault("width", "0"));

            int perCol = colspan > 0 ? w / colspan : 0;
            for (int k = gi; k < gi + colspan && k < bestGridCols; k++)
            {
                widths[k] = perCol;
                if (perCol == 0) autoIndices.Add(k);
                else usedFixed += perCol;
            }
            gi += colspan;
        }

        if (autoIndices.Count > 0)
        {
            int autoW = Math.Max(0, tableWidth - usedFixed) / autoIndices.Count;
            foreach (var idx in autoIndices) widths[idx] = autoW;
        }

        return [.. widths];
    }

    private TableCell FooterCeldaPageNum(int cellWidth, Dictionary<string, string> css, string pageLabel, string pageTotalLabel, bool pageTotal, string? pageBgColor, string? pageColor, int defaultFontSizeHp, int colspan)
    {
        var hp = css.TryGetValue("font-size", out var fs) ? PtToHalfPt(fs) : defaultFontSizeHp;
        var lineH = (hp * 10).ToString();
        var (padT, padR, padB, padL) = ObtenerPadding(css);

        var tcPr = new TableCellProperties(
            new TableCellWidth { Width = cellWidth.ToString(), Type = TableWidthUnitValues.Dxa },
            new TableCellMargin(
                new TopMargin    { Width = padT.ToString(), Type = TableWidthUnitValues.Dxa },
                new BottomMargin { Width = padB.ToString(), Type = TableWidthUnitValues.Dxa },
                new LeftMargin   { Width = padL.ToString(), Type = TableWidthUnitValues.Dxa },
                new RightMargin  { Width = padR.ToString(), Type = TableWidthUnitValues.Dxa }),
            new TableCellVerticalAlignment { Val = TableVerticalAlignmentValues.Center });
        if (colspan > 1) tcPr.Append(new GridSpan { Val = colspan });
        if (!string.IsNullOrEmpty(pageBgColor))
            tcPr.Append(new Shading { Val = ShadingPatternValues.Clear, Color = "auto", Fill = NormalizarColor(pageBgColor) });

        var tc = new TableCell();
        tc.Append(tcPr);

        RunProperties BuildRp()
        {
            var rp = new RunProperties(new FontSize { Val = hp.ToString() }, new RunFonts { Ascii = _fontFamily, HighAnsi = _fontFamily });
            if (!string.IsNullOrEmpty(pageColor)) rp.Append(new Color { Val = NormalizarColor(pageColor) });
            return rp;
        }

        var para = new Paragraph();
        para.Append(new ParagraphProperties(
            new Justification { Val = JustificationValues.Center },
            new SpacingBetweenLines { Before = "0", After = "0", Line = lineH, LineRule = LineSpacingRuleValues.Exact }));

        var r1 = new Run(); r1.Append(BuildRp()); r1.Append(new Text(pageLabel + " ") { Space = SpaceProcessingModeValues.Preserve }); para.Append(r1);
        var r2 = new Run(); r2.Append(BuildRp()); r2.Append(new FieldChar { FieldCharType = FieldCharValues.Begin }); para.Append(r2);
        var r3 = new Run(); r3.Append(BuildRp()); r3.Append(new FieldCode(" PAGE ") { Space = SpaceProcessingModeValues.Preserve }); para.Append(r3);
        var r4 = new Run(); r4.Append(BuildRp()); r4.Append(new FieldChar { FieldCharType = FieldCharValues.End }); para.Append(r4);

        if (pageTotal)
        {
            var r5 = new Run(); r5.Append(BuildRp()); r5.Append(new Text(" " + pageTotalLabel + " ") { Space = SpaceProcessingModeValues.Preserve }); para.Append(r5);
            var r6 = new Run(); r6.Append(BuildRp()); r6.Append(new FieldChar { FieldCharType = FieldCharValues.Begin }); para.Append(r6);
            var r7 = new Run(); r7.Append(BuildRp()); r7.Append(new FieldCode(" NUMPAGES ") { Space = SpaceProcessingModeValues.Preserve }); para.Append(r7);
            var r8 = new Run(); r8.Append(BuildRp()); r8.Append(new FieldChar { FieldCharType = FieldCharValues.End }); para.Append(r8);
        }

        tc.Append(para);
        return tc;
    }

    private TableCell FooterCeldaTexto(string text, int cellWidth, Dictionary<string, string> css, int colspan, int defaultFontSizeHp)
    {
        var hp   = css.TryGetValue("font-size", out var fs) ? PtToHalfPt(fs) : defaultFontSizeHp;
        var lineH = (hp * 10).ToString();
        var bold = css.TryGetValue("font-weight", out var fw) && (fw == "bold" || fw == "700");
        var vAlign = css.TryGetValue("vertical-align", out var va) && va == "middle"
            ? TableVerticalAlignmentValues.Center : TableVerticalAlignmentValues.Top;
        var (padT, padR, padB, padL) = ObtenerPadding(css);

        var tcPr = new TableCellProperties(
            new TableCellWidth { Width = cellWidth.ToString(), Type = TableWidthUnitValues.Dxa },
            new TableCellMargin(
                new TopMargin    { Width = padT.ToString(), Type = TableWidthUnitValues.Dxa },
                new BottomMargin { Width = padB.ToString(), Type = TableWidthUnitValues.Dxa },
                new LeftMargin   { Width = padL.ToString(), Type = TableWidthUnitValues.Dxa },
                new RightMargin  { Width = padR.ToString(), Type = TableWidthUnitValues.Dxa }),
            new TableCellVerticalAlignment { Val = vAlign });
        if (colspan > 1) tcPr.Append(new GridSpan { Val = colspan });
        if (css.TryGetValue("background-color", out var bg))
            tcPr.Append(new Shading { Val = ShadingPatternValues.Clear, Color = "auto", Fill = NormalizarColor(bg) });

        var tc = new TableCell();
        tc.Append(tcPr);

        var para = new Paragraph();
        var pPr = new ParagraphProperties(
            new SpacingBetweenLines { Before = "0", After = "0", Line = lineH, LineRule = LineSpacingRuleValues.Exact });
        if (css.TryGetValue("text-align", out var ta)) pPr.Append(new Justification { Val = MapAlign(ta) });
        para.Append(pPr);

        var rp = new RunProperties(new FontSize { Val = hp.ToString() }, new RunFonts { Ascii = _fontFamily, HighAnsi = _fontFamily });
        if (bold) rp.Append(new Bold());
        if (css.TryGetValue("color", out var color)) rp.Append(new Color { Val = NormalizarColor(color) });

        var run = new Run();
        run.Append(rp);
        var lines = text.Replace("\r\n", "\n").Replace('\r', '\n').Split('\n');
        for (int i = 0; i < lines.Length; i++)
        {
            if (i > 0) run.Append(new Break());
            run.Append(new Text(lines[i]) { Space = SpaceProcessingModeValues.Preserve });
        }
        para.Append(run);
        tc.Append(para);
        return tc;
    }

    private TableCell FooterCeldaImagen(byte[]? imgBytes, int cellWidth, Dictionary<string, string> css, int colspan, string imgWidthStr, string imgHeightStr, FooterPart footerPart)
    {
        var vAlign = css.TryGetValue("vertical-align", out var va) && va == "middle"
            ? TableVerticalAlignmentValues.Center : TableVerticalAlignmentValues.Top;
        var (padT, padR, padB, padL) = ObtenerPadding(css);

        var tcPr = new TableCellProperties(
            new TableCellWidth { Width = cellWidth.ToString(), Type = TableWidthUnitValues.Dxa },
            new TableCellMargin(
                new TopMargin    { Width = padT.ToString(), Type = TableWidthUnitValues.Dxa },
                new BottomMargin { Width = padB.ToString(), Type = TableWidthUnitValues.Dxa },
                new LeftMargin   { Width = padL.ToString(), Type = TableWidthUnitValues.Dxa },
                new RightMargin  { Width = padR.ToString(), Type = TableWidthUnitValues.Dxa }),
            new TableCellVerticalAlignment { Val = vAlign });
        if (colspan > 1) tcPr.Append(new GridSpan { Val = colspan });

        var tc = new TableCell();
        tc.Append(tcPr);

        if (imgBytes is { Length: > 0 })
        {
            var targetW = !string.IsNullOrEmpty(imgWidthStr) ? CssToEmu(imgWidthStr) : CssToEmu("1in");
            var targetH = !string.IsNullOrEmpty(imgHeightStr) ? CssToEmu(imgHeightStr) : CssToEmu("0.5in");
            var (iW, iH) = AjustarImagenContain(imgBytes, targetW, targetH);

            var imgPart = imgBytes.Length >= 2 && imgBytes[0] == 0xFF && imgBytes[1] == 0xD8
                ? footerPart.AddImagePart(ImagePartType.Jpeg)
                : footerPart.AddImagePart(ImagePartType.Png);
            using (var s = new MemoryStream(imgBytes)) imgPart.FeedData(s);
            var relId   = footerPart.GetIdOfPart(imgPart);
            var drawId  = (uint)_nextDrawingId++;

            var picture = new PIC.Picture(
                new PIC.NonVisualPictureProperties(
                    new PIC.NonVisualDrawingProperties { Id = 0, Name = "footer-img.png" },
                    new PIC.NonVisualPictureDrawingProperties()),
                new PIC.BlipFill(new A.Blip { Embed = relId }, new A.Stretch(new A.FillRectangle())),
                new PIC.ShapeProperties(
                    new A.Transform2D(new A.Offset { X = 0, Y = 0 }, new A.Extents { Cx = iW, Cy = iH }),
                    new A.PresetGeometry(new A.AdjustValueList()) { Preset = A.ShapeTypeValues.Rectangle }));

            var align = css.TryGetValue("text-align", out var ta) ? MapAlign(ta) : JustificationValues.Right;
            var lineHeight = Math.Max(1, (int)Math.Ceiling(iH / 635.0));
            var para = new Paragraph();
            para.Append(new ParagraphProperties(
                new Justification { Val = align },
                new SpacingBetweenLines { Before = "0", After = "0", Line = lineHeight.ToString(), LineRule = LineSpacingRuleValues.AtLeast }));
            var run = new Run();
            run.Append(new RunProperties(new NoProof(), new FontSize { Val = "1" }));
            run.Append(new Drawing(
                new DW.Inline(
                    new DW.Extent { Cx = iW, Cy = iH },
                    new DW.EffectExtent { LeftEdge = 0, TopEdge = 0, RightEdge = 0, BottomEdge = 0 },
                    new DW.DocProperties { Id = drawId, Name = "footer-img" },
                    new DW.NonVisualGraphicFrameDrawingProperties(new A.GraphicFrameLocks { NoChangeAspect = true }),
                    new A.Graphic(new A.GraphicData(picture) { Uri = "http://schemas.openxmlformats.org/drawingml/2006/picture" }))));
            para.Append(run);
            tc.Append(para);
        }
        else
        {
            tc.Append(new Paragraph());
        }

        return tc;
    }

    private TableCell FooterCeldaConTablaInterna(JsonArray nestedRows, int cellWidth, Dictionary<string, string> css, int colspan, int defaultFontSizeHp, JsonNode? footerConfig, FooterPart footerPart)
    {
        var vAlign = css.TryGetValue("vertical-align", out var va) && va == "middle"
            ? TableVerticalAlignmentValues.Center : TableVerticalAlignmentValues.Top;

        var tcPr = new TableCellProperties(
            new TableCellWidth { Width = cellWidth.ToString(), Type = TableWidthUnitValues.Dxa },
            new TableCellMargin(
                new TopMargin    { Width = "0", Type = TableWidthUnitValues.Dxa },
                new BottomMargin { Width = "0", Type = TableWidthUnitValues.Dxa },
                new LeftMargin   { Width = "0", Type = TableWidthUnitValues.Dxa },
                new RightMargin  { Width = "0", Type = TableWidthUnitValues.Dxa }),
            new TableCellVerticalAlignment { Val = vAlign });
        if (colspan > 1) tcPr.Append(new GridSpan { Val = colspan });

        var tc = new TableCell();
        tc.Append(tcPr);
        tc.Append(ConstruirTablaFooterGenerico(nestedRows, cellWidth, 0, footerConfig, footerPart, aplicarContainerStyle: false));
        tc.Append(CrearParrafoCero());
        return tc;
    }

    private static Paragraph CrearParrafoCero()
    {
        return new Paragraph(
            new ParagraphProperties(
                new SpacingBetweenLines { Before = "0", After = "0", Line = "1", LineRule = LineSpacingRuleValues.Exact }),
            new Run(new RunProperties(new FontSize { Val = "1" }), new Text("")));
    }

    private static Paragraph CrearParrafoAlto(int height)
    {
        return new Paragraph(
            new ParagraphProperties(
                new SpacingBetweenLines { Before = "0", After = "0", Line = height.ToString(), LineRule = LineSpacingRuleValues.Exact }),
            new Run(new RunProperties(new FontSize { Val = "1" }), new Text("")));
    }

    private static void AplicarContainerStyleFooter(TableProperties tableProperties, JsonNode? footerConfig)
    {
        var css = ParseCss(footerConfig?["containerStyle"]?.GetValue<string>());
        if (css.Count == 0) return;

        if (css.TryGetValue("background-color", out var bgColor))
            tableProperties.Append(new Shading { Val = ShadingPatternValues.Clear, Color = "auto", Fill = NormalizarColor(bgColor) });

        if (css.TryGetValue("border", out var border) && EsBordeVisible(border))
        {
            var (size, color) = ObtenerBorde(border);
            tableProperties.Append(new TableBorders(
                CrearTopBorder(BorderValues.Single, size, color, false),
                CrearBottomBorder(BorderValues.Single, size, color, false),
                CrearLeftBorder(BorderValues.Single, size, color, false),
                CrearRightBorder(BorderValues.Single, size, color, false),
                new InsideHorizontalBorder { Val = BorderValues.Nil },
                new InsideVerticalBorder { Val = BorderValues.Nil }));
            return;
        }

        css.TryGetValue("border-top", out var top);
        css.TryGetValue("border-bottom", out var bottom);
        css.TryGetValue("border-left", out var left);
        css.TryGetValue("border-right", out var right);
        if (top == null && bottom == null && left == null && right == null) return;

        tableProperties.Append(new TableBorders(
            top != null && EsBordeVisible(top) ? CrearTopBorder(BorderValues.Single, ObtenerBorde(top).Size, ObtenerBorde(top).Color, false) : new TopBorder { Val = BorderValues.Nil },
            bottom != null && EsBordeVisible(bottom) ? CrearBottomBorder(BorderValues.Single, ObtenerBorde(bottom).Size, ObtenerBorde(bottom).Color, false) : new BottomBorder { Val = BorderValues.Nil },
            left != null && EsBordeVisible(left) ? CrearLeftBorder(BorderValues.Single, ObtenerBorde(left).Size, ObtenerBorde(left).Color, false) : new LeftBorder { Val = BorderValues.Nil },
            right != null && EsBordeVisible(right) ? CrearRightBorder(BorderValues.Single, ObtenerBorde(right).Size, ObtenerBorde(right).Color, false) : new RightBorder { Val = BorderValues.Nil },
            new InsideHorizontalBorder { Val = BorderValues.Nil },
            new InsideVerticalBorder { Val = BorderValues.Nil }));
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
        var headerMarginTop = CssToTwips(config["header"]?["marginTop"]?.GetValue<string>() ?? "0");
        var headerDistance = Math.Max(0, mt - logoHeight - headerGap - headerMarginTop);
        var footerGapBefore = CssToTwips(config["footer"]?["gapBefore"]?.GetValue<string>() ?? "0");
        var footerMarginBottom = CssToTwips(config["footer"]?["marginBottom"]?.GetValue<string>() ?? "0");
        var footerLayout = config["footer"]?["layout"]?.GetValue<string>() ?? "";
        var footerRows = config["footer"]?["rows"]?.AsArray();
        var footerDistance = footerLayout == "table" && footerMarginBottom == 0 && footerRows != null
            ? Math.Max(0, mb - EstimarAltoTablaFooter(footerRows, config["footer"]) - footerGapBefore)
            : footerMarginBottom;

        var secPr = new SectionProperties();
        secPr.Append(new PageSize { Width = (uint)pageW, Height = (uint)pageH });
        secPr.Append(new PageMargin
        {
            Top = mt,
            Bottom = mb,
            Left = (uint)ml,
            Right = (uint)mr,
            Header = (uint)headerDistance,
            Footer = (uint)footerDistance
        });

        // Page border
        var pageBorderNode = config["pageBorder"];
        if (pageBorderNode is JsonObject)
        {
            var bWidth = pageBorderNode["width"]?.GetValue<string>() ?? "1pt";
            var bColor = pageBorderNode["color"]?.GetValue<string>() ?? "#000000";
            var (bSize, _) = ObtenerBorde($"{bWidth} solid {bColor}");
            var color = NormalizarColor(bColor);
            var spTop = (uint)(CssToTwips(pageBorderNode["top"]?.GetValue<string>() ?? "24pt") / 20);
            var spBottom = (uint)(CssToTwips(pageBorderNode["bottom"]?.GetValue<string>() ?? "24pt") / 20);
            var spLeft = (uint)(CssToTwips(pageBorderNode["left"]?.GetValue<string>() ?? "24pt") / 20);
            var spRight = (uint)(CssToTwips(pageBorderNode["right"]?.GetValue<string>() ?? "24pt") / 20);
            secPr.Append(new PageBorders(
                new TopBorder { Val = BorderValues.Single, Size = bSize, Space = spTop, Color = color },
                new BottomBorder { Val = BorderValues.Single, Size = bSize, Space = spBottom, Color = color },
                new LeftBorder { Val = BorderValues.Single, Size = bSize, Space = spLeft, Color = color },
                new RightBorder { Val = BorderValues.Single, Size = bSize, Space = spRight, Color = color }
            ) { OffsetFrom = PageBorderOffsetValues.Page });
        }

        // Link header
        var headerPart = mainPart.HeaderParts.FirstOrDefault(p => p != _firstHeaderPart);
        if (headerPart != null)
            secPr.InsertAt(new HeaderReference { Type = HeaderFooterValues.Default, Id = mainPart.GetIdOfPart(headerPart) }, 0);

        if (_firstHeaderPart != null)
        {
            secPr.InsertAt(new HeaderReference { Type = HeaderFooterValues.First, Id = mainPart.GetIdOfPart(_firstHeaderPart) }, 0);
            if (_firstFooterPart is null)
                secPr.InsertAt(new TitlePage(), 0);
        }

        // Link footer(s)
        if (_defaultFooterPart != null)
        {
            secPr.InsertAt(new FooterReference { Type = HeaderFooterValues.Default, Id = mainPart.GetIdOfPart(_defaultFooterPart) }, 0);
        }

        if (_firstFooterPart != null)
        {
            secPr.InsertAt(new FooterReference { Type = HeaderFooterValues.First, Id = mainPart.GetIdOfPart(_firstFooterPart) }, 0);
            secPr.InsertAt(new TitlePage(), 0);
        }

        body.Append(secPr);
    }

    private static int EstimarAltoTablaFooter(JsonArray rows, JsonNode? footerConfig)
    {
        var defaultFontSizeHp = PtToHalfPt(footerConfig?["fontSize"]?.GetValue<string>() ?? "11pt");
        var total = 0;

        foreach (var rowNode in rows)
        {
            if (rowNode is not JsonObject rowObj) continue;
            var cells = rowObj["cells"]?.AsArray();
            if (cells is null || cells.Count == 0) continue;

            var rowHeight = 0;
            foreach (var cell in cells)
            {
                if (cell is null) continue;
                rowHeight = Math.Max(rowHeight, EstimarAltoCeldaFooter(cell, defaultFontSizeHp, footerConfig));
            }
            total += rowHeight;
        }

        return total;
    }

    private static int EstimarAltoCeldaFooter(JsonNode cell, int defaultFontSizeHp, JsonNode? footerConfig)
    {
        var css = ParseCss(cell["style"]?.GetValue<string>());
        var (padT, _, padB, _) = ObtenerPadding(css);

        var contentHeight = 0;
        if (cell["rows"] is JsonArray nestedRows)
        {
            contentHeight = EstimarAltoTablaFooter(nestedRows, footerConfig);
        }
        else if (!string.IsNullOrWhiteSpace(cell["image"]?.GetValue<string>()))
        {
            contentHeight = CssToTwips(cell["imageHeight"]?.GetValue<string>() ?? "0.35in");
        }
        else
        {
            var hp = css.TryGetValue("font-size", out var fs) ? PtToHalfPt(fs) : defaultFontSizeHp;
            contentHeight = hp * 10;
        }

        return padT + contentHeight + padB;
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
            tPr.Append(new TableWidth { Width = "0", Type = TableWidthUnitValues.Auto });
        }

        var alignment = ObtenerAlineacionTabla(css);
        if (isFixed)
        {
            var remainingWidth = Math.Max(0, _contentWidth - tableWidth);
            var alignmentOffset = alignment == TableRowAlignmentValues.Center
                ? remainingWidth / 2
                : alignment == TableRowAlignmentValues.Right
                    ? remainingWidth
                    : 0;
            var tableIndent = _contentIndentL + alignmentOffset;
            tPr.Append(new TableJustification { Val = TableRowAlignmentValues.Left });
            tPr.Append(new TableIndentation { Width = tableIndent, Type = TableWidthUnitValues.Dxa });
        }
        else
        {
            tPr.Append(new TableJustification { Val = alignment });
        }

        // Borders
        bool esTablaOutset = false;
        var tableBorderShadow = css.ContainsKey("box-shadow");
        var collapseBorders = EsBorderCollapseCollapse(css);
        if (css.TryGetValue("border", out var tableBorder) && EsBordeVisible(tableBorder))
        {
            var (size, color) = ObtenerBorde(tableBorder);
            var borders = new TableBorders(
                CrearTopBorder(BorderValues.Single, size, color, tableBorderShadow),
                CrearBottomBorder(BorderValues.Single, size, color, tableBorderShadow),
                CrearLeftBorder(BorderValues.Single, size, color, tableBorderShadow),
                CrearRightBorder(BorderValues.Single, size, color, tableBorderShadow));
            if (collapseBorders)
            {
                borders.Append(new InsideHorizontalBorder { Val = BorderValues.Single, Size = size, Space = 0, Color = color });
                borders.Append(new InsideVerticalBorder { Val = BorderValues.Single, Size = size, Space = 0, Color = color });
            }
            tPr.Append(borders);
        }
        else
        {
            css.TryGetValue("border-top", out var tblTop);
            css.TryGetValue("border-bottom", out var tblBottom);
            css.TryGetValue("border-left", out var tblLeft);
            css.TryGetValue("border-right", out var tblRight);
            if (tblTop != null || tblBottom != null || tblLeft != null || tblRight != null)
            {
                var topColor = tblTop != null ? ObtenerBorde(tblTop).Color : null;
                var bottomColor = tblBottom != null ? ObtenerBorde(tblBottom).Color : null;
                var leftColor = tblLeft != null ? ObtenerBorde(tblLeft).Color : null;
                var rightColor = tblRight != null ? ObtenerBorde(tblRight).Color : null;
                var refBorde = tblBottom ?? tblTop ?? tblRight ?? tblLeft!;
                var (tblSize, _) = ObtenerBorde(refBorde);

                esTablaOutset = EsColorClaro(topColor) && EsColorClaro(leftColor)
                              && !EsColorClaro(bottomColor) && !EsColorClaro(rightColor);
                var tblVal = esTablaOutset ? BorderValues.ThreeDEmboss : BorderValues.Single;
                var tblCol = esTablaOutset ? "auto" : (bottomColor ?? topColor ?? "000000");
                var tblSizeDocx = esTablaOutset ? ObtenerTamanoBordeOutsetDocx(css, tblSize) : tblSize;
                var borders = new TableBorders(
                    CrearTopBorder(tblVal, tblSizeDocx, tblCol, tableBorderShadow),
                    CrearBottomBorder(tblVal, tblSizeDocx, tblCol, tableBorderShadow),
                    CrearLeftBorder(tblVal, tblSizeDocx, tblCol, tableBorderShadow),
                    CrearRightBorder(tblVal, tblSizeDocx, tblCol, tableBorderShadow));
                if (collapseBorders)
                {
                    borders.Append(new InsideHorizontalBorder { Val = tblVal, Size = tblSizeDocx, Space = 0, Color = tblCol });
                    borders.Append(new InsideVerticalBorder { Val = tblVal, Size = tblSizeDocx, Space = 0, Color = tblCol });
                }
                tPr.Append(borders);
            }
        }

        // Cell spacing and background: outset/inset borders render their own 3D gap natively in Word,
        // so only apply tblCellSpacing and tblShd for non-3D tables.
        if (!esTablaOutset)
        {
            if (css.TryGetValue("border-spacing", out var borderSpacing))
            {
                var spacingTwips = CssToTwips(borderSpacing.Trim().Split(' ')[0]);
                if (spacingTwips > 0)
                    tPr.Append(new TableCellSpacing { Width = spacingTwips.ToString(), Type = TableWidthUnitValues.Dxa });
            }
            if (css.TryGetValue("background-color", out var tblBgColor))
                tPr.Append(new Shading { Val = ShadingPatternValues.Clear, Color = "auto", Fill = NormalizarColor(tblBgColor) });
        }

        if (isFixed)
            tPr.Append(new TableLayout { Type = TableLayoutValues.Fixed });


        table.Append(tPr);
        return table;
    }

    private TableCell CrearCeldaTexto(string text, int widthTwips, Dictionary<string, string>? css,
        int colspan = 0, byte[]? imageBytes = null)
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
            css.TryGetValue("border-top", out var top);
            css.TryGetValue("border-bottom", out var bottom);
            css.TryGetValue("border-left", out var left);
            css.TryGetValue("border-right", out var right);

            if (top != null || bottom != null || left != null || right != null)
            {
                var topColor = top != null ? ObtenerBorde(top).Color : null;
                var bottomColor = bottom != null ? ObtenerBorde(bottom).Color : null;
                var leftColor = left != null ? ObtenerBorde(left).Color : null;
                var rightColor = right != null ? ObtenerBorde(right).Color : null;
                var refBorde = top ?? bottom ?? left ?? right!;
                var (cellSize, _) = ObtenerBorde(refBorde);

                var esInset = !EsColorClaro(topColor) && !EsColorClaro(leftColor)
                            && EsColorClaro(bottomColor) && EsColorClaro(rightColor);
                if (!esInset)
                {
                    var borders = new TableCellBorders();
                    if (top != null && EsBordeVisible(top))
                    {
                        var (s, c) = ObtenerBorde(top);
                        borders.Append(new TopBorder { Val = BorderValues.Single, Size = s, Space = 0, Color = c });
                    }
                    if (bottom != null && EsBordeVisible(bottom))
                    {
                        var (s, c) = ObtenerBorde(bottom);
                        borders.Append(new BottomBorder { Val = BorderValues.Single, Size = s, Space = 0, Color = c });
                    }
                    if (left != null && EsBordeVisible(left))
                    {
                        var (s, c) = ObtenerBorde(left);
                        borders.Append(new LeftBorder { Val = BorderValues.Single, Size = s, Space = 0, Color = c });
                    }
                    if (right != null && EsBordeVisible(right))
                    {
                        var (s, c) = ObtenerBorde(right);
                        borders.Append(new RightBorder { Val = BorderValues.Single, Size = s, Space = 0, Color = c });
                    }
                    if (borders.HasChildren)
                        tcPr.Append(borders);
                }
            }
        }

        if (css != null && css.TryGetValue("background-color", out var bgColor))
            tcPr.Append(new Shading { Val = ShadingPatternValues.Clear, Color = "auto", Fill = NormalizarColor(bgColor) });

        var (padTop, padRight, padBottom, padLeft) = ObtenerPadding(css);
        var cellVerticalAlign = ObtenerAlineacionVerticalCelda(css);
        var useParagraphTopPadding = cellVerticalAlign == TableVerticalAlignmentValues.Top;
        tcPr.Append(new TableCellMargin(
            new TopMargin { Width = useParagraphTopPadding ? "0" : padTop.ToString(), Type = TableWidthUnitValues.Dxa },
            new BottomMargin { Width = padBottom.ToString(), Type = TableWidthUnitValues.Dxa },
            new LeftMargin { Width = padLeft.ToString(), Type = TableWidthUnitValues.Dxa },
            new RightMargin { Width = padRight.ToString(), Type = TableWidthUnitValues.Dxa }
        ));

        tcPr.Append(new TableCellVerticalAlignment { Val = cellVerticalAlign });
        tc.Append(tcPr);

        var hp = css != null && css.TryGetValue("font-size", out var fs) ? PtToHalfPt(fs) : _fontSizeHp;

        // Paragraph inside cell
        var para = new Paragraph();
        var pPr = new ParagraphProperties();
        pPr.Append(new SpacingBetweenLines
        {
            Before = useParagraphTopPadding ? padTop.ToString() : "0",
            After = "0",
            Line = CssLineSpacing(css, hp).ToString(),
            LineRule = LineSpacingRuleValues.Exact
        });

        if (css != null && css.TryGetValue("text-align", out var align))
            pPr.Append(new Justification { Val = MapAlign(align) });

        para.Append(pPr);

        // Image in cell: inline (mode 2 / block-centered) or anchored (mode 3 / independent)
        if (imageBytes is not null && imageBytes.Length > 0 && _mainPart is not null)
        {
            var bgSize = css?.GetValueOrDefault("background-size", "auto auto") ?? "auto auto";
            var isContainSize = bgSize.Trim().Equals("contain", StringComparison.OrdinalIgnoreCase);
            var sizeParts = bgSize.Trim().Split(' ');
            var hPart = sizeParts.Length >= 2 ? sizeParts[1] : "auto";
            var wPart = sizeParts.Length >= 1 ? sizeParts[0] : "auto";

            long targetH, targetW = long.MaxValue / 2;
            if (isContainSize)
            {
                targetW = widthTwips > 0 ? widthTwips * 635L : long.MaxValue / 2;
                targetH = CssToEmu(css?.GetValueOrDefault("height", "") ?? "");
                if (targetH <= 0)
                {
                    var lineHEmu = (long)(hp / 2.0 * 12700 * _lineSpacingMultiplier);
                    targetH = lineHEmu + (long)((padTop + padBottom) * 635L);
                }
            }
            else if (hPart.EndsWith("%") && double.TryParse(hPart.TrimEnd('%'),
                    System.Globalization.NumberStyles.Any,
                    System.Globalization.CultureInfo.InvariantCulture, out var pct))
            {
                var lineHEmu = (long)(hp / 2.0 * 12700 * _lineSpacingMultiplier);
                var paddingEmu = (long)((padTop + padBottom) * 635L);
                targetH = (long)((lineHEmu + paddingEmu) * (pct / 100.0));
            }
            else if (!hPart.Equals("auto", StringComparison.OrdinalIgnoreCase))
                targetH = CssToEmu(hPart);
            else
            {
                var lineHEmu = (long)(hp / 2.0 * 12700 * _lineSpacingMultiplier);
                targetH = (long)((lineHEmu + (long)((padTop + padBottom) * 635L)) * 0.7);
            }

            if (!isContainSize)
            {
                targetW = wPart.Equals("auto", StringComparison.OrdinalIgnoreCase)
                    ? long.MaxValue / 2
                    : CssToEmu(wPart);
            }

            var (imgW, imgH) = AjustarImagenContain(imageBytes, targetW, targetH);

            var imgPart = imageBytes.Length >= 2 && imageBytes[0] == 0xFF && imageBytes[1] == 0xD8
                ? _mainPart.AddImagePart(ImagePartType.Jpeg)
                : _mainPart.AddImagePart(ImagePartType.Png);
            using (var imgStream = new MemoryStream(imageBytes))
                imgPart.FeedData(imgStream);

            var relId = _mainPart.GetIdOfPart(imgPart);
            var drawId = _nextDrawingId++;

            var bgPosition = ObtenerPosicionFondoDocx(
                css?.GetValueOrDefault("background-position", "center center"),
                widthTwips,
                imgW,
                imgH);
            var isMode3 = isContainSize
                || !bgPosition.XKeyword.Equals("center", StringComparison.OrdinalIgnoreCase)
                || !bgPosition.YKeyword.Equals("center", StringComparison.OrdinalIgnoreCase);

            var picture = new PIC.Picture(
                new PIC.NonVisualPictureProperties(
                    new PIC.NonVisualDrawingProperties { Id = 0, Name = "image.png" },
                    new PIC.NonVisualPictureDrawingProperties()),
                new PIC.BlipFill(
                    new A.Blip { Embed = relId },
                    new A.Stretch(new A.FillRectangle())),
                new PIC.ShapeProperties(
                    new A.Transform2D(
                        new A.Offset { X = 0, Y = 0 },
                        new A.Extents { Cx = imgW, Cy = imgH }),
                    new A.PresetGeometry(new A.AdjustValueList())
                        { Preset = A.ShapeTypeValues.Rectangle }));

            var graphic = new A.Graphic(
                new A.GraphicData(picture)
                    { Uri = "http://schemas.openxmlformats.org/drawingml/2006/picture" });

            Drawing imgDrawing;
            if (isMode3)
            {
                OpenXmlElement verticalPositionChild = bgPosition.YKeyword.Equals("center", StringComparison.OrdinalIgnoreCase)
                    ? new DW.VerticalAlignment { Text = "center" }
                    : new DW.PositionOffset(bgPosition.YOffsetEmu.ToString());

                imgDrawing = new Drawing(
                    new DW.Anchor(
                        new DW.SimplePosition { X = 0, Y = 0 },
                        new DW.HorizontalPosition(new DW.PositionOffset(bgPosition.XOffsetEmu.ToString()))
                            { RelativeFrom = DW.HorizontalRelativePositionValues.Column },
                        new DW.VerticalPosition(verticalPositionChild)
                            { RelativeFrom = DW.VerticalRelativePositionValues.Paragraph },
                        new DW.Extent { Cx = imgW, Cy = imgH },
                        new DW.EffectExtent { LeftEdge = 0, TopEdge = 0, RightEdge = 0, BottomEdge = 0 },
                        new DW.WrapNone(),
                        new DW.DocProperties { Id = drawId, Name = $"img{drawId}" },
                        new DW.NonVisualGraphicFrameDrawingProperties(
                            new A.GraphicFrameLocks { NoChangeAspect = true }),
                        graphic)
                    {
                        DistanceFromTop = 0, DistanceFromBottom = 0,
                        DistanceFromLeft = 0, DistanceFromRight = 0,
                        SimplePos = false, RelativeHeight = 1,
                        BehindDoc = false, Locked = false,
                        LayoutInCell = true, AllowOverlap = false
                    });
            }
            else
            {
                imgDrawing = new Drawing(
                    new DW.Inline(
                        new DW.Extent { Cx = imgW, Cy = imgH },
                        new DW.EffectExtent { LeftEdge = 0, TopEdge = 0, RightEdge = 0, BottomEdge = 0 },
                        new DW.DocProperties { Id = drawId, Name = $"img{drawId}" },
                        new DW.NonVisualGraphicFrameDrawingProperties(
                            new A.GraphicFrameLocks { NoChangeAspect = true }),
                        graphic)
                    { DistanceFromTop = 0, DistanceFromBottom = 0, DistanceFromLeft = 0, DistanceFromRight = 0 });
            }

            para.Append(new Run(imgDrawing));
        }

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

    private static void AplicarAltoFila(TableRow row, int heightTwips)
    {
        if (heightTwips <= 0) return;

        var properties = row.GetFirstChild<TableRowProperties>();
        if (properties == null)
        {
            properties = new TableRowProperties();
            row.PrependChild(properties);
        }

        properties.Append(new TableRowHeight
        {
            Val = (uint)heightTwips,
            HeightType = HeightRuleValues.Exact
        });
    }

    private static (string XKeyword, long XOffsetEmu, string YKeyword, long YOffsetEmu) ObtenerPosicionFondoDocx(
        string? backgroundPosition,
        int cellWidthTwips,
        long imageWidthEmu,
        long imageHeightEmu)
    {
        var parts = (backgroundPosition ?? "center center")
            .Trim()
            .Split(' ', StringSplitOptions.RemoveEmptyEntries);

        var xKeyword = parts.Length > 0 ? parts[0] : "center";
        var xInset = parts.Length > 1 && EsLongitudCss(parts[1]) ? CssToEmu(parts[1]) : 0;
        var yKeyword = "center";
        var yInset = 0L;

        if (parts.Length >= 4)
        {
            yKeyword = parts[2];
            yInset = EsLongitudCss(parts[3]) ? CssToEmu(parts[3]) : 0;
        }
        else if (parts.Length >= 2 && !EsLongitudCss(parts[1]))
        {
            yKeyword = parts[1];
        }

        var cellWidthEmu = cellWidthTwips > 0 ? cellWidthTwips * 635L : 0;
        var xOffset = xKeyword.ToLowerInvariant() switch
        {
            "right" when cellWidthEmu > 0 => Math.Max(0, cellWidthEmu - imageWidthEmu - xInset),
            "center" when cellWidthEmu > 0 => Math.Max(0, (cellWidthEmu - imageWidthEmu) / 2),
            "left" => xInset,
            _ when EsLongitudCss(xKeyword) => CssToEmu(xKeyword),
            _ => 0
        };

        var yOffset = yKeyword.ToLowerInvariant() switch
        {
            "top" => yInset,
            _ when EsLongitudCss(yKeyword) => CssToEmu(yKeyword),
            _ => 0
        };

        return (xKeyword, xOffset, yKeyword, yOffset);
    }

    private static bool EsLongitudCss(string value) =>
        Regex.IsMatch(value, @"^[\d.]+\s*(in|pt|px)?$", RegexOptions.IgnoreCase);

    private static TableVerticalAlignmentValues ObtenerAlineacionVerticalCelda(Dictionary<string, string>? css)
    {
        var verticalAlign = css?.GetValueOrDefault("vertical-align")?.ToLowerInvariant();
        return verticalAlign switch
        {
            "middle" => TableVerticalAlignmentValues.Center,
            "center" => TableVerticalAlignmentValues.Center,
            "bottom" => TableVerticalAlignmentValues.Bottom,
            _ => TableVerticalAlignmentValues.Top
        };
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
        Dictionary<string, string>? child,
        bool inheritBorder = true)
    {
        string[] inheritedProperties =
        [
            "color", "font-family", "font-size", "font-style", "font-weight",
            "line-height", "text-align", "text-decoration", "white-space", "padding", "border"
        ];

        var result = new Dictionary<string, string>();
        foreach (var property in inheritedProperties)
        {
            if (property == "border" && !inheritBorder)
                continue;
            if (parent.TryGetValue(property, out var value))
                result[property] = value;
        }

        if (child != null)
            foreach (var pair in child)
                result[pair.Key] = pair.Value;

        return result;
    }

    private static bool EsBorderCollapseCollapse(Dictionary<string, string> css) =>
        css.GetValueOrDefault("border-collapse", "").Equals("collapse", StringComparison.OrdinalIgnoreCase);

    private static bool HeredarBordeTablaEnCeldas(Dictionary<string, string> css) =>
        EsBorderCollapseCollapse(css);

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

    private static uint ObtenerTamanoBordeOutsetDocx(Dictionary<string, string> css, uint borderSize)
    {
        if (!css.TryGetValue("border-spacing", out var spacing))
            return borderSize * 4;

        var firstValue = spacing.Trim().Split(' ', StringSplitOptions.RemoveEmptyEntries).FirstOrDefault();
        var spacingTwips = CssToTwips(firstValue ?? "");
        if (spacingTwips <= 0)
            return borderSize * 4;

        var spacingBorderUnits = (uint)Math.Max(0, Math.Round(spacingTwips * 8.0 / 20.0));
        return Math.Max(1u, borderSize + spacingBorderUnits);
    }

    private static TopBorder CrearTopBorder(BorderValues val, uint size, string color, bool shadow)
    {
        var border = new TopBorder { Val = val, Size = size, Space = 0, Color = color };
        AplicarSombraBorde(border, shadow);
        return border;
    }

    private static BottomBorder CrearBottomBorder(BorderValues val, uint size, string color, bool shadow)
    {
        var border = new BottomBorder { Val = val, Size = size, Space = 0, Color = color };
        AplicarSombraBorde(border, shadow);
        return border;
    }

    private static LeftBorder CrearLeftBorder(BorderValues val, uint size, string color, bool shadow)
    {
        var border = new LeftBorder { Val = val, Size = size, Space = 0, Color = color };
        AplicarSombraBorde(border, shadow);
        return border;
    }

    private static RightBorder CrearRightBorder(BorderValues val, uint size, string color, bool shadow)
    {
        var border = new RightBorder { Val = val, Size = size, Space = 0, Color = color };
        AplicarSombraBorde(border, shadow);
        return border;
    }

    private static void AplicarSombraBorde(BorderType border, bool shadow)
    {
        if (!shadow) return;

        border.Shadow = true;
        border.Frame = true;
    }

    private static string NormalizarColor(string value)
    {
        var color = value.Trim().TrimStart('#');
        if (color.Length == 3)
            color = string.Concat(color.Select(c => $"{c}{c}"));
        return Regex.IsMatch(color, "^[0-9a-fA-F]{6}$") ? color.ToUpperInvariant() : "000000";
    }

    private static bool EsColorClaro(string? color)
    {
        if (color == null) return false;
        var c = NormalizarColor(color);
        if (!int.TryParse(c, System.Globalization.NumberStyles.HexNumber, null, out var rgb)) return false;
        var r = (rgb >> 16) & 0xFF;
        var g = (rgb >> 8) & 0xFF;
        var b = rgb & 0xFF;
        return (r * 299 + g * 587 + b * 114) / 1000 > 180;
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

    private void ColapsarMargenParrafoAnterior(Body body, ParagraphProperties properties, Dictionary<string, string> css)
    {
        if (_lastBodyParagraph == null || !ReferenceEquals(body.LastChild, _lastBodyParagraph))
            return;

        var (marginTop, _) = ObtenerMargenVertical(css);
        if (_lastBodyMarginBottom <= 0 && marginTop <= 0) return;

        var prevSpacing = _lastBodyParagraph
            .GetFirstChild<ParagraphProperties>()?
            .GetFirstChild<SpacingBetweenLines>();
        if (prevSpacing != null)
            prevSpacing.After = _lastBodyPaddingBottom.ToString();

        var (paddingTop, _) = ObtenerPaddingVertical(css);
        var spacing = properties.GetFirstChild<SpacingBetweenLines>();
        if (spacing != null)
            spacing.Before = (paddingTop + Math.Max(_lastBodyMarginBottom, marginTop)).ToString();
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
        var m = Regex.Match(value, @"([\d.]+)\s*(in|pt|px|)");
        if (!m.Success || !double.TryParse(m.Groups[1].Value, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out var num))
            return 0;
        return m.Groups[2].Value switch
        {
            "in" => (int)(num * 1440),
            "pt" => (int)(num * 20),
            "px" => (int)(num * 15),
            _ => (int)(num * 1440)
        };
    }

    private static long CssToEmu(string value)
    {
        var m = Regex.Match(value, @"([\d.]+)\s*(in|pt|px|)");
        if (!m.Success || !double.TryParse(m.Groups[1].Value, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out var num))
            return 0;
        return m.Groups[2].Value switch
        {
            "in" => (long)(num * 914400),
            "pt" => (long)(num * 12700),
            "px" => (long)(num * 9525),
            _ => (long)(num * 914400)
        };
    }
}
