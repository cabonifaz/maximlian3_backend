using System.Text.Json.Nodes;
using System.Text.RegularExpressions;
using PdfSharp;
using PdfSharp.Drawing;
using PdfSharp.Drawing.Layout;
using PdfSharp.Fonts;
using PdfSharp.Pdf;

namespace SafetyReport.Handlers;

public class PdfGeneratorService
{
    private string _fontFamily = "Calibri";
    private double _fontSize = 10;
    private double _lineSpacingMultiplier = 1.15;
    private double _contentIndentL = 0;
    private double _contentIndentR = 0;
    private double _contentWidth = 0;
    private byte[]? _logoBytes;
    private byte[]? _watermarkBytes;
    private double _wmWidth, _wmHeight, _wmOpacity;
    private string _wmPosition = "";

    private double _pageW, _pageH;
    private double _mTop, _mBottom, _mLeft, _mRight;
    private double _logoBoxW, _logoBoxH;
    private double _headerGapAfter;
    private string _headerAlign = "center";
    private string _footerText = "";
    private string _pageLabel = "Page";
    private double _footerFontSize = 7;
    private string _footerAlign = "left";
    private double _footerGapBefore = 0;
    private double _footerIndentL = 0;
    private double _footerIndentR = 0;
    private bool _showPageNumber = true;
    private double _pageFontSize;
    private double _pageGapBefore;
    private XColor _pageColor;
    private bool _hasPageColor;
    private bool _hasPageBorder;
    private double _pageBorderWidth;
    private XColor _pageBorderColor;
    private double _pageBorderTop, _pageBorderBottom, _pageBorderLeft, _pageBorderRight;

    private PdfDocument _doc = null!;
    private XGraphics _gfx = null!;
    private double _y;
    private double _contentTop;
    private double _contentBottom;
    private int _pageNumber;
    private readonly HashSet<string> _drawnBorderLines = [];

    private double _lastMarginBottom;
    private double _lastPaddingBottom;
    private double _lastAppliedBottomSpacing;
    private double _pendingTableBottomMargin;
    private bool _hasLastParagraph;

    private static S3FontResolver? _fontResolver;
    private static readonly object _fontLock = new();

    public static void ConfigurarFuentes(Dictionary<string, byte[]> fuentes)
    {
        lock (_fontLock)
        {
            if (_fontResolver == null)
            {
                _fontResolver = new S3FontResolver(fuentes);
                GlobalFontSettings.FontResolver = _fontResolver;
            }
            else
            {
                _fontResolver.AgregarFuentes(fuentes);
            }
        }
    }

    public MemoryStream GenerarPdf(JsonNode json, byte[]? logoBytes = null, byte[]? watermarkBytes = null)
    {
        _doc = new PdfDocument();
        _logoBytes = logoBytes;
        _watermarkBytes = watermarkBytes;
        _pendingTableBottomMargin = 0;
        _lastMarginBottom = 0;
        _lastPaddingBottom = 0;
        _lastAppliedBottomSpacing = 0;
        _hasLastParagraph = false;
        _pageNumber = 0;

        var config = json["document"];
        LeerConfigGlobal(config);
        NuevaPagina();

        var sections = json["sections"]?.AsArray();
        if (sections != null)
            foreach (var section in sections)
                if (section != null) RenderizarSeccion(section);

        FlushPendingTableMargin();

        var ms = new MemoryStream();
        _gfx.Dispose();
        _doc.Save(ms);
        ms.Position = 0;
        return ms;
    }

    private void LeerConfigGlobal(JsonNode? config)
    {
        if (config is null) return;
        _fontFamily = config["font"]?["family"]?.GetValue<string>()?.Split(',')[0].Trim() ?? "Calibri";
        _fontSize = PtValue(config["font"]?["size"]?.GetValue<string>() ?? "10pt");
        var ls = config["font"]?["lineSpacing"]?.GetValue<double>() ?? 1.15;
        if (ls > 10) ls /= 100.0;
        _lineSpacingMultiplier = ls;
        _contentIndentL = CssToPoints(config["contentIndent"]?["left"]?.GetValue<string>() ?? "0");
        _contentIndentR = CssToPoints(config["contentIndent"]?["right"]?.GetValue<string>() ?? "0");

        _pageW = CssToPoints(config["pageSize"]?["width"]?.GetValue<string>() ?? "8.27in");
        _pageH = CssToPoints(config["pageSize"]?["height"]?.GetValue<string>() ?? "11.69in");
        _mTop = CssToPoints(config["margins"]?["top"]?.GetValue<string>() ?? "1.15in");
        _mBottom = CssToPoints(config["margins"]?["bottom"]?.GetValue<string>() ?? "1.0in");
        _mLeft = CssToPoints(config["margins"]?["left"]?.GetValue<string>() ?? "0.5in");
        _mRight = CssToPoints(config["margins"]?["right"]?.GetValue<string>() ?? "0.5in");
        _contentWidth = _pageW - _mLeft - _mRight - _contentIndentL - _contentIndentR;

        _logoBoxW = CssToPoints(config["header"]?["logoWidth"]?.GetValue<string>() ?? "1.3in");
        _logoBoxH = CssToPoints(config["header"]?["logoHeight"]?.GetValue<string>() ?? "0.55in");
        _headerGapAfter = CssToPoints(config["header"]?["gapAfter"]?.GetValue<string>() ?? "0");
        _headerAlign = config["header"]?["align"]?.GetValue<string>() ?? "center";

        _footerText = config["footer"]?["text"]?.GetValue<string>() ?? "";
        _pageLabel = config["footer"]?["pageLabel"]?.GetValue<string>() ?? "Page";
        _footerFontSize = PtValue(config["footer"]?["fontSize"]?.GetValue<string>() ?? "7pt");
        _footerAlign = config["footer"]?["align"]?.GetValue<string>() ?? "left";
        _footerGapBefore = CssToPoints(config["footer"]?["gapBefore"]?.GetValue<string>() ?? "0");
        _footerIndentL = CssToPoints(config["footerIndent"]?["left"]?.GetValue<string>() ?? "0");
        _footerIndentR = CssToPoints(config["footerIndent"]?["right"]?.GetValue<string>() ?? "0");
        _showPageNumber = config["footer"]?["showPageNumber"]?.GetValue<bool>() ?? true;
        _pageFontSize = PtValue(config["footer"]?["pageFontSize"]?.GetValue<string>() ?? config["footer"]?["fontSize"]?.GetValue<string>() ?? "7pt");
        _pageGapBefore = CssToPoints(config["footer"]?["pageGapBefore"]?.GetValue<string>() ?? "0");
        var pageColorHex = config["footer"]?["pageColor"]?.GetValue<string>();
        if (!string.IsNullOrEmpty(pageColorHex))
        {
            _hasPageColor = true;
            var pc = pageColorHex.TrimStart('#');
            if (pc.Length == 3) pc = string.Concat(pc.Select(ch => $"{ch}{ch}"));
            _pageColor = XColor.FromArgb(
                Convert.ToInt32(pc[..2], 16),
                Convert.ToInt32(pc[2..4], 16),
                Convert.ToInt32(pc[4..6], 16));
        }

        _contentTop = _mTop;
        _contentBottom = _pageH - _mBottom;

        var wmNode = config["watermark"];
        if (wmNode is JsonObject)
        {
            _wmWidth = CssToPoints(wmNode["width"]?.GetValue<string>() ?? "0");
            _wmHeight = CssToPoints(wmNode["height"]?.GetValue<string>() ?? "0");
            _wmOpacity = wmNode["opacity"]?.GetValue<double>() ?? 1.0;
            _wmPosition = wmNode["position"]?.GetValue<string>() ?? "center center";
        }

        var pageBorderNode = config["pageBorder"];
        if (pageBorderNode is JsonObject)
        {
            _hasPageBorder = true;
            _pageBorderWidth = PtValue(pageBorderNode["width"]?.GetValue<string>() ?? "1pt");
            var colorHex = pageBorderNode["color"]?.GetValue<string>() ?? "#000000";
            var c = colorHex.TrimStart('#');
            if (c.Length == 3) c = string.Concat(c.Select(ch => $"{ch}{ch}"));
            _pageBorderColor = XColor.FromArgb(
                Convert.ToInt32(c[..2], 16),
                Convert.ToInt32(c[2..4], 16),
                Convert.ToInt32(c[4..6], 16));
            _pageBorderTop = CssToPoints(pageBorderNode["top"]?.GetValue<string>() ?? "24pt");
            _pageBorderBottom = CssToPoints(pageBorderNode["bottom"]?.GetValue<string>() ?? "24pt");
            _pageBorderLeft = CssToPoints(pageBorderNode["left"]?.GetValue<string>() ?? "24pt");
            _pageBorderRight = CssToPoints(pageBorderNode["right"]?.GetValue<string>() ?? "24pt");
        }
    }

    // ==================== PAGE MANAGEMENT ====================

    private void NuevaPagina()
    {
        if (_gfx != null) _gfx.Dispose();

        var page = _doc.AddPage();
        page.Width = XUnit.FromPoint(_pageW);
        page.Height = XUnit.FromPoint(_pageH);
        _gfx = XGraphics.FromPdfPage(page);
        _pageNumber++;
        _y = _contentTop;
        _drawnBorderLines.Clear();

        DibujarMarcaAgua();
        DibujarEncabezado();
        DibujarPiePagina();
        DibujarBordePagina();
    }

    private bool AsegurarEspacio(double height)
    {
        if (_y + height > _contentBottom)
        {
            NuevaPagina();
            return true;
        }
        return false;
    }

    private void DibujarMarcaAgua()
    {
        if (_watermarkBytes is null || _watermarkBytes.Length == 0 || _wmWidth <= 0 || _wmHeight <= 0) return;

        try
        {
            using var imgStream = new MemoryStream(_watermarkBytes);
            var image = XImage.FromStream(imgStream);

            var scaleW = _wmWidth / image.PointWidth;
            var scaleH = _wmHeight / image.PointHeight;
            var scale = Math.Min(scaleW, scaleH);
            var imgW = image.PointWidth * scale;
            var imgH = image.PointHeight * scale;

            var parts = _wmPosition.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            var hPos = parts.Length > 0 ? parts[0] : "center";
            var vPos = parts.Length > 1 ? parts[1] : "center";

            var x = hPos switch
            {
                "left" => 0.0,
                "right" => _pageW - imgW,
                _ => (_pageW - imgW) / 2
            };
            var y = vPos switch
            {
                "top" => 0.0,
                "bottom" => _pageH - imgH,
                _ => (_pageH - imgH) / 2
            };

            _gfx.DrawImage(image, x, y, imgW, imgH);
        }
        catch { }
    }

    private void DibujarEncabezado()
    {
        if (_logoBytes is null || _logoBytes.Length == 0) return;

        try
        {
            using var imgStream = new MemoryStream(_logoBytes);
            var image = XImage.FromStream(imgStream);

            var scaleW = _logoBoxW / image.PointWidth;
            var scaleH = _logoBoxH / image.PointHeight;
            var scale = Math.Min(scaleW, scaleH);
            var logoW = image.PointWidth * scale;
            var logoH = image.PointHeight * scale;

            var headerTop = _mTop - _headerGapAfter - _logoBoxH;
            var logoY = headerTop + (_logoBoxH - logoH) / 2;
            var logoX = _headerAlign switch
            {
                "left" => _mLeft,
                "right" => _pageW - _mRight - logoW,
                _ => _mLeft + (_pageW - _mLeft - _mRight - logoW) / 2
            };

            _gfx.DrawImage(image, logoX, logoY, logoW, logoH);
        }
        catch { }
    }

    private void DibujarPiePagina()
    {
        var footerFont = CrearFuente(null, _footerFontSize);
        var lineH = _footerFontSize;
        var footerX = _mLeft + _footerIndentL;
        var footerW = _pageW - _mLeft - _mRight - _footerIndentL - _footerIndentR;
        var footerY = _contentBottom + _footerGapBefore;
        var align = MapXAlign(_footerAlign);

        if (!string.IsNullOrEmpty(_footerText))
        {
            var lines = PartirEnLineas(_footerText, footerFont, footerW);
            foreach (var line in lines)
            {
                DibujarLineaTexto(line, footerFont, footerX, footerW, footerY, align, lineH);
                footerY += lineH;
            }
        }

        if (_showPageNumber)
        {
            footerY += _pageGapBefore;
            var pageFont = CrearFuente(null, _pageFontSize);
            var pageLineH = _pageFontSize;
            var pageBrush = _hasPageColor ? new XSolidBrush(_pageColor) : XBrushes.Black;
            var pageText = $"{_pageLabel} {_pageNumber}";
            var ascent = pageFont.Size * pageFont.Metrics.Ascent / pageFont.Metrics.UnitsPerEm;
            var descent = pageFont.Size * Math.Abs(pageFont.Metrics.Descent) / pageFont.Metrics.UnitsPerEm;
            var textHeight = ascent + descent;
            var baselineY = footerY + (pageLineH - textHeight) / 2 + ascent;
            var format = new XStringFormat { Alignment = align, LineAlignment = XLineAlignment.BaseLine };
            _gfx.DrawString(pageText, pageFont, pageBrush, new XPoint(align == XStringAlignment.Center ? footerX + footerW / 2 : align == XStringAlignment.Far ? footerX + footerW : footerX, baselineY), format);
        }
    }

    private void DibujarBordePagina()
    {
        if (!_hasPageBorder) return;
        var pen = new XPen(_pageBorderColor, _pageBorderWidth);
        var x = _pageBorderLeft;
        var y = _pageBorderTop;
        var w = _pageW - _pageBorderLeft - _pageBorderRight;
        var h = _pageH - _pageBorderTop - _pageBorderBottom;
        _gfx.DrawRectangle(pen, x, y, w, h);
    }

    // ==================== SECTION RENDERERS ====================

    private void RenderizarSeccion(JsonNode section)
    {
        var type = section["type"]?.GetValue<string>() ?? "";
        switch (type)
        {
            case "heading": RenderHeading(section); break;
            case "subtitle": RenderSubtitle(section); break;
            case "text": RenderText(section); break;
            case "keyValue": RenderKeyValue(section); break;
            case "borderedBox": RenderBorderedBox(section); break;
            case "referenceBox": RenderReferenceBox(section); break;
            case "dataTable": RenderDataTable(section); break;
            case "repeat":
                var subs = section["sections"]?.AsArray();
                if (subs != null)
                    foreach (var sub in subs)
                        if (sub != null) RenderizarSeccion(sub);
                break;
            case "repeatDetail": RenderRepeatDetail(section); break;
            case "spacer": RenderSpacer(section); break;
        }
    }

    private void RenderHeading(JsonNode section)
    {
        var text = section["text"]?.GetValue<string>() ?? "";
        var css = ParseCss(section["style"]?.GetValue<string>());
        var font = CrearFuente(css);
        var lineH = LineHeight(css);
        var (beforeM, afterM) = ObtenerMargenVertical(css);
        var (beforeP, afterP) = ObtenerPaddingVertical(css);
        var before = beforeM + beforeP;
        var after = afterM + afterP;

        ColapsarMargenParrafoAnterior(css, ref before);
        AplicarMargenPendienteTabla(css, ref before);

        var textH = lineH;
        AsegurarEspacio(before + textH);

        _y += before;
        var x = _mLeft + _contentIndentL;
        var w = _contentWidth;
        var align = MapXAlign(css.GetValueOrDefault("text-align", "left"));
        DibujarLineaTexto(text, font, x, w, _y, align, lineH);
        _y += textH;
        _y += after;

        RegistrarParrafo(afterM, afterP, after);
    }

    private void RenderSubtitle(JsonNode section)
    {
        var text = section["text"]?.GetValue<string>() ?? "";
        var css = ParseCss(section["style"]?.GetValue<string>());
        var font = CrearFuente(css);
        var lineH = LineHeight(css);
        var (beforeM, afterM) = ObtenerMargenVertical(css);
        var (beforeP, afterP) = ObtenerPaddingVertical(css);
        var before = beforeM + beforeP;
        var after = afterM + afterP;

        ColapsarMargenParrafoAnterior(css, ref before);
        AplicarMargenPendienteTabla(css, ref before);

        var textH = lineH;
        AsegurarEspacio(before + textH);

        _y += before;
        var x = _mLeft + _contentIndentL;
        var align = MapXAlign(css.GetValueOrDefault("text-align", "left"));
        DibujarLineaTexto(text, font, x, _contentWidth, _y, align, lineH);
        _y += textH;
        _y += after;

        RegistrarParrafo(afterM, afterP, after);
    }

    private void RenderText(JsonNode section)
    {
        var text = section["field"]?.GetValue<string>() ?? "";
        var css = ParseCss(section["style"]?.GetValue<string>());
        if (string.IsNullOrEmpty(text)) return;

        var font = CrearFuente(css);
        var lineH = LineHeight(css);
        var x = _mLeft + _contentIndentL;
        var w = _contentWidth;
        var (_, afterM) = ObtenerMargenVertical(css);
        var (_, afterP) = ObtenerPaddingVertical(css);
        // DOCX text paragraphs start with Before/After = 0. Their CSS margins
        // participate only when adjacent paragraph margins are collapsed.
        var before = 0d;

        ColapsarMargenParrafoAnterior(css, ref before);
        AplicarMargenPendienteTabla(css, ref before);
        _y += before;

        var whiteSpace = css.GetValueOrDefault("white-space", "normal");
        var preserveBreaks = whiteSpace is "pre-line" or "pre-wrap" or "pre";
        var preserveSpaces = whiteSpace is "pre-wrap" or "pre";

        var rawLines = preserveBreaks
            ? text.Replace("\r\n", "\n").Replace('\r', '\n').Split('\n')
            : [Regex.Replace(text, @"\s+", " ").Trim()];

        foreach (var rawLine in rawLines)
        {
            var line = preserveSpaces ? rawLine : Regex.Replace(rawLine, @"[\t ]+", " ").Trim();
            var wrapped = PartirEnLineas(line, font, w);
            foreach (var wl in wrapped)
            {
                AsegurarEspacio(lineH);
                DibujarLineaTexto(wl, font, x, w, _y, XStringAlignment.Near, lineH);
                _y += lineH;
            }
        }

        RegistrarParrafo(afterM, afterP, appliedBottomSpacing: 0);
    }

    private void RenderKeyValue(JsonNode section)
    {
        var tblCss = ParseCss(section["style"]?.GetValue<string>());
        var lblCss = ParseCss(section["labelStyle"]?.GetValue<string>());
        var rows = section["rows"]?.AsArray();
        if (rows is null || rows.Count == 0) return;

        var effectiveLblCss = CombinarCssHeredable(tblCss, lblCss);
        var effectiveValCss = CombinarCssHeredable(tblCss, null);
        var tblW = ObtenerAnchoTabla(tblCss);
        var lblW = CssToPoints(lblCss.GetValueOrDefault("width", ""));
        var valW = lblW > 0 && tblW > lblW ? tblW - lblW : tblW / 2;
        if (lblW <= 0) lblW = tblW - valW;

        var tableRows = new List<TableRowData>();
        foreach (var row in rows)
        {
            if (row is null) continue;
            var label = row["label"]?.GetValue<string>() ?? "";
            var value = row["value"]?.GetValue<string>() ?? "";
            var sep = row["separator"]?.GetValue<string>() ?? "";
            tableRows.Add(new TableRowData([
                new CellData(label, lblW, effectiveLblCss),
                new CellData(sep + value, valW, effectiveValCss)
            ]));
        }

        DibujarTabla(tableRows, tblW, tblCss);
    }

    private void RenderBorderedBox(JsonNode section)
    {
        var tblCss = ParseCss(section["style"]?.GetValue<string>());
        var titleCss = ParseCss(section["titleStyle"]?.GetValue<string>());
        var lblCss = ParseCss(section["labelStyle"]?.GetValue<string>());
        var valCss = ParseCss(section["valueStyle"]?.GetValue<string>());
        var cellCss = ParseCss(section["cellStyle"]?.GetValue<string>());
        var title = section["title"]?.GetValue<string>() ?? "";
        var content = section["content"]?.GetValue<string>();
        var rows = section["rows"]?.AsArray();

        var tblW = ObtenerAnchoTabla(tblCss);
        var lblW = CssToPoints(lblCss.GetValueOrDefault("width", ""));
        if (lblW <= 0 || lblW >= tblW) lblW = tblW / 2;
        var valW = tblW - lblW;

        var tableRows = new List<TableRowData>();

        tableRows.Add(new TableRowData([new CellData(title, tblW, CombinarCssHeredable(tblCss, titleCss), Colspan: 2)]));

        if (!string.IsNullOrEmpty(content))
            tableRows.Add(new TableRowData([new CellData(content, tblW, CombinarCssHeredable(tblCss, cellCss), Colspan: 2)]));

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
                var effectiveLblW = CssToPoints(rowCss.GetValueOrDefault("width", ""));
                if (effectiveLblW == 0) effectiveLblW = lblW;
                var effectiveValW = effectiveLblW > 0 && tblW > effectiveLblW ? tblW - effectiveLblW : valW;

                tableRows.Add(new TableRowData([
                    new CellData(label, effectiveLblW, effectiveLblCss),
                    new CellData(value, effectiveValW, effectiveValCss)
                ]));
            }
        }

        DibujarTabla(tableRows, tblW, tblCss);
    }

    private void RenderReferenceBox(JsonNode section)
    {
        var tblCss = ParseCss(section["style"]?.GetValue<string>());
        var titleCss = ParseCss(section["titleStyle"]?.GetValue<string>());
        var cellCss = ParseCss(section["cellStyle"]?.GetValue<string>());
        var lastCellCss = ParseCss(section["lastCellStyle"]?.GetValue<string>());
        var title = section["title"]?.GetValue<string>() ?? "";
        var items = section["items"]?.AsArray();

        var tblW = ObtenerAnchoTabla(tblCss);
        var tableRows = new List<TableRowData>();

        tableRows.Add(new TableRowData([new CellData(title, tblW, CombinarCssHeredable(tblCss, titleCss))]));

        if (items != null)
        {
            for (int i = 0; i < items.Count; i++)
            {
                var text = items[i]?.GetValue<string>() ?? "";
                var isLast = i == items.Count - 1;
                var rawCss = isLast && lastCellCss.Count > 0 ? lastCellCss : cellCss;
                tableRows.Add(new TableRowData([new CellData(text, tblW, CombinarCssHeredable(tblCss, rawCss))]));
            }
        }

        DibujarTabla(tableRows, tblW, tblCss);
    }

    private void RenderDataTable(JsonNode section)
    {
        var tblCss = ParseCss(section["style"]?.GetValue<string>());
        var cellCss = ParseCss(section["cellStyle"]?.GetValue<string>());
        var headerCss = ParseCss(section["headerStyle"]?.GetValue<string>());
        var columns = section["columns"]?.AsArray();
        var rows = section["rows"]?.AsArray();
        var columnWidths = section["columnWidths"]?.AsArray();

        if (columns is null) return;

        var effectiveCellCss = CombinarCssHeredable(tblCss, cellCss);
        var mergedHeaderCss = new Dictionary<string, string>(effectiveCellCss)
        {
            ["font-weight"] = "bold",
            ["text-align"] = "center"
        };
        foreach (var kv in headerCss) mergedHeaderCss[kv.Key] = kv.Value;

        var tblW = ObtenerAnchoTabla(tblCss);
        var colWidths = new double[columns.Count];
        for (int i = 0; i < columns.Count; i++)
            colWidths[i] = columnWidths != null && i < columnWidths.Count
                ? CssToPoints(columnWidths[i]?.GetValue<string>() ?? "") : 0;
        colWidths = NormalizarAnchos(colWidths, tblW);

        var tableRows = new List<TableRowData>();

        var headerRow = new List<CellData>();
        for (int i = 0; i < columns.Count; i++)
        {
            var header = columns[i]?["header"]?.GetValue<string>() ?? "";
            headerRow.Add(new CellData(header, colWidths[i], mergedHeaderCss));
        }
        tableRows.Add(new TableRowData(headerRow, IsHeader: true));

        if (rows != null)
        {
            foreach (var row in rows)
            {
                if (row is null) continue;
                var values = new List<string>();
                if (row is JsonArray arr)
                    foreach (var v in arr) values.Add(v?.GetValue<string>() ?? "");
                else if (row is JsonObject obj)
                    foreach (var prop in obj) values.Add(prop.Value?.GetValue<string>() ?? "");

                var cells = new List<CellData>();
                for (int i = 0; i < values.Count; i++)
                {
                    var w = i < colWidths.Length ? colWidths[i] : tblW / columns.Count;
                    cells.Add(new CellData(values[i], w, effectiveCellCss));
                }
                tableRows.Add(new TableRowData(cells));
            }
        }

        DibujarTabla(tableRows, tblW, tblCss);
    }

    private void RenderRepeatDetail(JsonNode section)
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

            var titleFont = CrearFuente(titleCss);
            var titleLineH = LineHeight(titleCss);
            var (tBeforeM, tAfterM) = ObtenerMargenVertical(titleCss);
            var (tBeforeP, tAfterP) = ObtenerPaddingVertical(titleCss);
            var before = tBeforeM + tBeforeP;
            var after = tAfterM + tAfterP;

            ColapsarMargenParrafoAnterior(titleCss, ref before);
            AplicarMargenPendienteTabla(titleCss, ref before);

            AsegurarEspacio(before + titleLineH);
            _y += before;
            DibujarLineaTexto(title, titleFont, _mLeft + _contentIndentL, _contentWidth, _y, XStringAlignment.Near, titleLineH);
            _y += titleLineH + after;
            RegistrarParrafo(tAfterM, tAfterP, after);

            if (!string.IsNullOrEmpty(content))
            {
                var contentFont = CrearFuente(contentCss);
                var contentLineH = LineHeight(contentCss);
                var (_, cAfterM) = ObtenerMargenVertical(contentCss);
                var (_, cAfterP) = ObtenerPaddingVertical(contentCss);
                // Same paragraph properties used by DOCX RenderRepeatDetail content.
                var cBefore = 0d;

                ColapsarMargenParrafoAnterior(contentCss, ref cBefore);
                AplicarMargenPendienteTabla(contentCss, ref cBefore);
                _y += cBefore;

                var whiteSpace = contentCss.GetValueOrDefault("white-space", "normal");
                var preserveBreaks = whiteSpace is "pre-line" or "pre-wrap" or "pre";
                var preserveSpaces = whiteSpace is "pre-wrap" or "pre";
                var rawLines = preserveBreaks
                    ? content.Replace("\r\n", "\n").Replace('\r', '\n').Split('\n')
                    : [Regex.Replace(content, @"\s+", " ").Trim()];

                foreach (var rawLine in rawLines)
                {
                    var line = preserveSpaces ? rawLine : Regex.Replace(rawLine, @"[\t ]+", " ").Trim();
                    foreach (var wl in PartirEnLineas(line, contentFont, _contentWidth))
                    {
                        AsegurarEspacio(contentLineH);
                        DibujarLineaTexto(wl, contentFont, _mLeft + _contentIndentL, _contentWidth, _y, XStringAlignment.Near, contentLineH);
                        _y += contentLineH;
                    }
                }

                RegistrarParrafo(cAfterM, cAfterP, appliedBottomSpacing: 0);
            }
        }
    }

    private void RenderSpacer(JsonNode section)
    {
        var height = CssToPoints(section["height"]?.GetValue<string>() ?? "0.3in");
        FlushPendingTableMargin();
        AsegurarEspacio(height);
        _y += height;
        LimpiarParrafoAnterior();
    }

    // ==================== TABLE DRAWING ====================

    private void DibujarTabla(List<TableRowData> rows, double tableWidth, Dictionary<string, string> tblCss)
    {
        if (rows.Count == 0) return;

        rows = rows.Select(row => NormalizarAnchosFila(row, tableWidth)).ToList();
        var (beforeM, afterM) = ObtenerMargenVertical(tblCss);

        var before = beforeM;
        if (_pendingTableBottomMargin > 0)
        {
            before = Math.Max(_pendingTableBottomMargin, before);
            _pendingTableBottomMargin = 0;
        }
        else if (_hasLastParagraph)
        {
            _y -= _lastAppliedBottomSpacing;
            before = _lastPaddingBottom + Math.Max(_lastMarginBottom, before);
            LimpiarParrafoAnterior();
        }

        _y += before;

        var alignment = ObtenerAlineacionTabla(tblCss);
        var tableX = alignment switch
        {
            "center" => _mLeft + _contentIndentL + (_contentWidth - tableWidth) / 2,
            "right" => _mLeft + _contentIndentL + _contentWidth - tableWidth,
            _ => _mLeft + _contentIndentL
        };

        var tableStartY = _y;
        var segmentRows = new List<TableRowData>();

        for (var rowIndex = 0; rowIndex < rows.Count; rowIndex++)
        {
            var row = rows[rowIndex];
            var rowH = MedirAltoFila(row, tableWidth);

            if (_y + rowH > _contentBottom && _y > _contentTop)
            {
                DibujarBordeTabla(tableX, tableStartY, tableWidth, _y - tableStartY, tblCss, segmentRows);
                NuevaPagina();
                tableStartY = _y;
                segmentRows.Clear();

                // DOCX marks data-table headers with w:tblHeader, so repeat the
                // same header when the manually drawn PDF table changes page.
                var header = rows[0];
                if (rowIndex > 0 && header.IsHeader)
                {
                    var headerH = MedirAltoFila(header, tableWidth);
                    DibujarFila(header, tableX, tableWidth, headerH);
                    _y += headerH;
                    segmentRows.Add(header);
                }
            }

            DibujarFila(row, tableX, tableWidth, rowH);
            _y += rowH;
            segmentRows.Add(row);
        }

        DibujarBordeTabla(tableX, tableStartY, tableWidth, _y - tableStartY, tblCss, segmentRows);

        _pendingTableBottomMargin = afterM;
        LimpiarParrafoAnterior();
    }

    private void DibujarFila(TableRowData row, double tableX, double tableWidth, double rowH)
    {
        var cellX = tableX;
        foreach (var cell in row.Cells)
        {
            var cellW = cell.Colspan > 1 ? tableWidth : cell.Width;
            var css = cell.Css;
            var (padT, padR, padB, padL) = ObtenerPadding(css);
            var font = CrearFuente(css);
            var lineH = LineHeight(css);
            var textW = Math.Max(0, cellW - padL - padR);
            var align = MapXAlign(css.GetValueOrDefault("text-align", "left"));

            var lines = PartirEnLineas(cell.Text, font, textW);
            var textY = _y + padT;
            foreach (var line in lines)
            {
                DibujarLineaTexto(line, font, cellX + padL, textW, textY, align, lineH);
                textY += lineH;
            }

            DibujarBordesCelda(cellX, _y, cellW, rowH, css);
            cellX += cellW;
        }
    }

    private static TableRowData NormalizarAnchosFila(TableRowData row, double tableWidth)
    {
        if (row.Cells.Count == 0 || row.Cells.Any(cell => cell.Colspan > 1)) return row;

        var normalized = NormalizarAnchos(row.Cells.Select(cell => cell.Width).ToArray(), tableWidth);
        var cells = row.Cells
            .Select((cell, index) => cell with { Width = normalized[index] })
            .ToList();
        return row with { Cells = cells };
    }

    private static double[] NormalizarAnchos(double[] widths, double tableWidth)
    {
        if (widths.Length == 0) return widths;

        var positiveTotal = widths.Where(width => width > 0).Sum();
        var missingCount = widths.Count(width => width <= 0);
        var result = widths.ToArray();

        if (positiveTotal <= 0)
            return Enumerable.Repeat(tableWidth / widths.Length, widths.Length).ToArray();

        if (missingCount > 0)
        {
            var remaining = Math.Max(0, tableWidth - positiveTotal);
            var fallback = remaining > 0
                ? remaining / missingCount
                : positiveTotal / (widths.Length - missingCount);
            for (var i = 0; i < result.Length; i++)
                if (result[i] <= 0) result[i] = fallback;
        }

        var total = result.Sum();
        if (total <= 0) return Enumerable.Repeat(tableWidth / widths.Length, widths.Length).ToArray();

        var scale = tableWidth / total;
        for (var i = 0; i < result.Length; i++) result[i] *= scale;
        return result;
    }

    private double MedirAltoFila(TableRowData row, double tableWidth)
    {
        double maxH = 0;
        foreach (var cell in row.Cells)
        {
            var cellW = cell.Colspan > 1 ? tableWidth : cell.Width;
            var css = cell.Css;
            var (padT, padR, padB, padL) = ObtenerPadding(css);
            var font = CrearFuente(css);
            var lineH = LineHeight(css);
            var textW = Math.Max(0, cellW - padL - padR);
            var lines = PartirEnLineas(cell.Text, font, textW);
            var cellH = padT + Math.Max(1, lines.Count) * lineH + padB;
            maxH = Math.Max(maxH, cellH);
        }
        return maxH;
    }

    private void DibujarBordeTabla(
        double x,
        double y,
        double w,
        double h,
        Dictionary<string, string> tblCss,
        IReadOnlyList<TableRowData> segmentRows)
    {
        if (h <= 0 || !tblCss.TryGetValue("border", out var border) || !EsBordeVisible(border)) return;
        var pen = CrearPen(border);
        if (!BordeExteriorCubierto(segmentRows, "top"))
            DibujarLineaBorde(pen, x, y, x + w, y);
        if (!BordeExteriorCubierto(segmentRows, "bottom"))
            DibujarLineaBorde(pen, x, y + h, x + w, y + h);
        if (!BordeExteriorCubierto(segmentRows, "left"))
            DibujarLineaBorde(pen, x, y, x, y + h);
        if (!BordeExteriorCubierto(segmentRows, "right"))
            DibujarLineaBorde(pen, x + w, y, x + w, y + h);
    }

    private static bool BordeExteriorCubierto(IReadOnlyList<TableRowData> rows, string side)
    {
        if (rows.Count == 0) return false;

        return side switch
        {
            "top" => rows[0].Cells.Count > 0 && rows[0].Cells.All(cell => TieneBorde(cell.Css, "top")),
            "bottom" => rows[^1].Cells.Count > 0 && rows[^1].Cells.All(cell => TieneBorde(cell.Css, "bottom")),
            "left" => rows.All(row => row.Cells.Count > 0 && TieneBorde(row.Cells[0].Css, "left")),
            "right" => rows.All(row => row.Cells.Count > 0 && TieneBorde(row.Cells[^1].Css, "right")),
            _ => false
        };
    }

    private static bool TieneBorde(Dictionary<string, string> css, string side) =>
        css.TryGetValue("border", out var all) && EsBordeVisible(all) ||
        css.TryGetValue($"border-{side}", out var specific) && EsBordeVisible(specific);

    private void DibujarBordesCelda(double x, double y, double w, double h, Dictionary<string, string> css)
    {
        if (css.TryGetValue("border", out var allBorder) && EsBordeVisible(allBorder))
        {
            var pen = CrearPen(allBorder);
            DibujarLineaBorde(pen, x, y, x + w, y);
            DibujarLineaBorde(pen, x, y + h, x + w, y + h);
            DibujarLineaBorde(pen, x, y, x, y + h);
            DibujarLineaBorde(pen, x + w, y, x + w, y + h);
            return;
        }

        if (css.TryGetValue("border-top", out var bt) && EsBordeVisible(bt))
            DibujarLineaBorde(CrearPen(bt), x, y, x + w, y);
        if (css.TryGetValue("border-bottom", out var bb) && EsBordeVisible(bb))
            DibujarLineaBorde(CrearPen(bb), x, y + h, x + w, y + h);
        if (css.TryGetValue("border-left", out var bl) && EsBordeVisible(bl))
            DibujarLineaBorde(CrearPen(bl), x, y, x, y + h);
        if (css.TryGetValue("border-right", out var br) && EsBordeVisible(br))
            DibujarLineaBorde(CrearPen(br), x + w, y, x + w, y + h);
    }

    private void DibujarLineaBorde(XPen pen, double x1, double y1, double x2, double y2)
    {
        var ax = Math.Round(Math.Min(x1, x2), 3);
        var ay = Math.Round(Math.Min(y1, y2), 3);
        var bx = Math.Round(Math.Max(x1, x2), 3);
        var by = Math.Round(Math.Max(y1, y2), 3);
        var key = FormattableString.Invariant($"{ax:F3},{ay:F3}:{bx:F3},{by:F3}");
        if (_drawnBorderLines.Add(key))
            _gfx.DrawLine(pen, x1, y1, x2, y2);
    }

    private XPen CrearPen(string borderValue)
    {
        var (size, color) = ObtenerBorde(borderValue);
        return new XPen(XColor.FromArgb(
            int.Parse(color[..2], System.Globalization.NumberStyles.HexNumber),
            int.Parse(color[2..4], System.Globalization.NumberStyles.HexNumber),
            int.Parse(color[4..6], System.Globalization.NumberStyles.HexNumber)), size);
    }

    // ==================== MARGIN COLLAPSING ====================

    private void ColapsarMargenParrafoAnterior(Dictionary<string, string> css, ref double before)
    {
        if (!_hasLastParagraph) return;

        var (marginTop, _) = ObtenerMargenVertical(css);
        var (paddingTop, _) = ObtenerPaddingVertical(css);

        // This is the same condition used by the DOCX generator. When neither
        // paragraph has a vertical margin, their original spacing is retained.
        if (_lastMarginBottom > 0 || marginTop > 0)
        {
            _y -= _lastAppliedBottomSpacing;
            before = _lastPaddingBottom + paddingTop + Math.Max(_lastMarginBottom, marginTop);
        }

        LimpiarParrafoAnterior();
    }

    private void AplicarMargenPendienteTabla(Dictionary<string, string> css, ref double before)
    {
        if (_pendingTableBottomMargin <= 0) return;

        var (marginTop, _) = ObtenerMargenVertical(css);
        var (paddingTop, _) = ObtenerPaddingVertical(css);
        before = paddingTop + Math.Max(_pendingTableBottomMargin, marginTop);
        _pendingTableBottomMargin = 0;
    }

    private void RegistrarParrafo(double marginBottom, double paddingBottom, double appliedBottomSpacing)
    {
        _hasLastParagraph = true;
        _lastMarginBottom = marginBottom;
        _lastPaddingBottom = paddingBottom;
        _lastAppliedBottomSpacing = appliedBottomSpacing;
    }

    private void LimpiarParrafoAnterior()
    {
        _hasLastParagraph = false;
        _lastMarginBottom = 0;
        _lastPaddingBottom = 0;
        _lastAppliedBottomSpacing = 0;
    }

    private void FlushPendingTableMargin()
    {
        if (_pendingTableBottomMargin <= 0) return;
        _y += _pendingTableBottomMargin;
        _pendingTableBottomMargin = 0;
        LimpiarParrafoAnterior();
    }

    // ==================== DRAWING HELPERS ====================

    private void DibujarLineaTexto(string text, XFont font, double x, double width, double y, XStringAlignment align, double lineHeight = 0)
    {
        if (string.IsNullOrEmpty(text)) return;
        var lh = lineHeight > 0 ? lineHeight : font.GetHeight();
        var ascent = font.Size * font.Metrics.Ascent / font.Metrics.UnitsPerEm;
        var descent = font.Size * Math.Abs(font.Metrics.Descent) / font.Metrics.UnitsPerEm;
        var textHeight = ascent + descent;
        var baselineY = y + (lh - textHeight) / 2 + ascent;
        var format = new XStringFormat { Alignment = align, LineAlignment = XLineAlignment.BaseLine };
        _gfx.DrawString(text, font, XBrushes.Black, new XPoint(align == XStringAlignment.Center ? x + width / 2 : align == XStringAlignment.Far ? x + width : x, baselineY), format);
    }

    private List<string> PartirEnLineas(string text, XFont font, double maxWidth)
    {
        if (string.IsNullOrEmpty(text)) return [""];
        if (maxWidth <= 0) return [text];

        var lines = new List<string>();
        var words = text.Split(' ');
        var currentLine = "";

        foreach (var word in words)
        {
            var testLine = currentLine.Length == 0 ? word : currentLine + " " + word;
            var size = _gfx.MeasureString(testLine, font);
            if (size.Width > maxWidth && currentLine.Length > 0)
            {
                lines.Add(currentLine);
                currentLine = word;
            }
            else
            {
                currentLine = testLine;
            }
        }
        if (currentLine.Length > 0) lines.Add(currentLine);
        if (lines.Count == 0) lines.Add("");
        return lines;
    }

    // ==================== FONT HELPERS ====================

    private XFont CrearFuente(Dictionary<string, string>? css, double? overrideSize = null)
    {
        var size = overrideSize ?? (css != null && css.TryGetValue("font-size", out var fs) ? PtValue(fs) : _fontSize);
        var family = css != null && css.TryGetValue("font-family", out var ff)
            ? ff.Split(',')[0].Trim().Trim('\'', '"') : _fontFamily;

        var style = XFontStyleEx.Regular;
        var weight = css?.GetValueOrDefault("font-weight");
        if (weight is "700" or "bold" || int.TryParse(weight, out var nw) && nw >= 600)
            style |= XFontStyleEx.Bold;
        if (css?.GetValueOrDefault("font-style") is "italic" or "oblique")
            style |= XFontStyleEx.Italic;

        return new XFont(family, size, style);
    }

    private double LineHeight(Dictionary<string, string>? css, double? fontSize = null)
    {
        var effectiveSize = fontSize ?? (css != null && css.TryGetValue("font-size", out var fs) ? PtValue(fs) : _fontSize);

        if (css != null && css.TryGetValue("line-height", out var lh))
        {
            var trimmed = lh.Trim();
            if (trimmed.EndsWith("pt", StringComparison.OrdinalIgnoreCase) ||
                trimmed.EndsWith("in", StringComparison.OrdinalIgnoreCase))
                return CssToPoints(trimmed);
            var m = Regex.Match(trimmed, @"([\d.]+)");
            if (m.Success && double.TryParse(m.Groups[1].Value,
                    System.Globalization.NumberStyles.Any,
                    System.Globalization.CultureInfo.InvariantCulture, out var multiplier))
                return effectiveSize * multiplier;
        }

        return effectiveSize * _lineSpacingMultiplier;
    }

    // ==================== CSS PARSING ====================

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

    private static double CssToPoints(string? value)
    {
        if (string.IsNullOrEmpty(value)) return 0;
        var m = Regex.Match(value, @"([\d.]+)\s*(in|pt|)");
        if (!m.Success || !double.TryParse(m.Groups[1].Value,
                System.Globalization.NumberStyles.Any,
                System.Globalization.CultureInfo.InvariantCulture, out var num))
            return 0;
        return m.Groups[2].Value switch
        {
            "in" => num * 72,
            "pt" => num,
            _ => num * 72
        };
    }

    private static double PtValue(string value)
    {
        var m = Regex.Match(value, @"([\d.]+)");
        return m.Success && double.TryParse(m.Groups[1].Value,
            System.Globalization.NumberStyles.Any,
            System.Globalization.CultureInfo.InvariantCulture, out var v) ? v : 10;
    }

    private static XStringAlignment MapXAlign(string align) => align switch
    {
        "center" => XStringAlignment.Center,
        "right" => XStringAlignment.Far,
        _ => XStringAlignment.Near
    };

    private Dictionary<string, string> CombinarCssHeredable(
        Dictionary<string, string> parent, Dictionary<string, string>? child)
    {
        string[] inherited =
        [
            "color", "font-family", "font-size", "font-style", "font-weight",
            "line-height", "text-align", "text-decoration", "white-space"
        ];
        var result = new Dictionary<string, string>();
        foreach (var prop in inherited)
            if (parent.TryGetValue(prop, out var val))
                result[prop] = val;
        if (child != null)
            foreach (var pair in child)
                result[pair.Key] = pair.Value;
        return result;
    }

    private double ObtenerAnchoTabla(Dictionary<string, string> css)
    {
        if (!css.TryGetValue("width", out var w)) return _contentWidth;
        if (w.Contains('%'))
        {
            var m = Regex.Match(w, @"([\d.]+)");
            var pct = m.Success && double.TryParse(m.Groups[1].Value,
                System.Globalization.NumberStyles.Any,
                System.Globalization.CultureInfo.InvariantCulture, out var p) ? p / 100.0 : 1.0;
            return _contentWidth * pct;
        }
        return CssToPoints(w);
    }

    private static string ObtenerAlineacionTabla(Dictionary<string, string> css)
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
        if (horizontal.Equals("auto", StringComparison.OrdinalIgnoreCase) || leftAuto && rightAuto) return "center";
        if (leftAuto) return "right";
        return "left";
    }

    private static (double Top, double Bottom) ObtenerMargenVertical(Dictionary<string, string> css) =>
        ObtenerEspaciadoVerticalCss(css, "margin");

    private static (double Top, double Bottom) ObtenerPaddingVertical(Dictionary<string, string> css) =>
        ObtenerEspaciadoVerticalCss(css, "padding");

    private static (double Top, double Bottom) ObtenerEspaciadoVerticalCss(
        Dictionary<string, string> css, string property)
    {
        double top = 0, bottom = 0;
        if (css.TryGetValue(property, out var shorthand))
        {
            var parts = shorthand.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length == 1) top = bottom = CssToPoints(parts[0]);
            else if (parts.Length == 2) top = bottom = CssToPoints(parts[0]);
            else if (parts.Length >= 3) { top = CssToPoints(parts[0]); bottom = CssToPoints(parts[2]); }
        }
        if (css.TryGetValue($"{property}-top", out var et)) top = CssToPoints(et);
        if (css.TryGetValue($"{property}-bottom", out var eb)) bottom = CssToPoints(eb);
        return (top, bottom);
    }

    private static (double Top, double Right, double Bottom, double Left) ObtenerPadding(Dictionary<string, string>? css)
    {
        double top = 0, right = 2.16, bottom = 0, left = 2.16;
        if (css != null && css.TryGetValue("padding", out var padding))
        {
            var parts = padding.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length == 1) top = right = bottom = left = CssToPoints(parts[0]);
            else if (parts.Length == 2) { top = bottom = CssToPoints(parts[0]); right = left = CssToPoints(parts[1]); }
            else if (parts.Length == 3) { top = CssToPoints(parts[0]); right = left = CssToPoints(parts[1]); bottom = CssToPoints(parts[2]); }
            else if (parts.Length >= 4) { top = CssToPoints(parts[0]); right = CssToPoints(parts[1]); bottom = CssToPoints(parts[2]); left = CssToPoints(parts[3]); }
        }
        if (css != null && css.TryGetValue("padding-top", out var pt)) top = CssToPoints(pt);
        if (css != null && css.TryGetValue("padding-right", out var pr)) right = CssToPoints(pr);
        if (css != null && css.TryGetValue("padding-bottom", out var pb)) bottom = CssToPoints(pb);
        if (css != null && css.TryGetValue("padding-left", out var pl)) left = CssToPoints(pl);
        return (top, right, bottom, left);
    }

    private static bool EsBordeVisible(string value) =>
        !string.IsNullOrEmpty(value) &&
        !value.Contains("none", StringComparison.OrdinalIgnoreCase) &&
        ObtenerBorde(value).Size > 0;

    private static (double Size, string Color) ObtenerBorde(string value)
    {
        var widthMatch = Regex.Match(value, @"([\d.]+)\s*(px|pt)", RegexOptions.IgnoreCase);
        double size = 0.75;
        if (widthMatch.Success && double.TryParse(widthMatch.Groups[1].Value,
                System.Globalization.NumberStyles.Any,
                System.Globalization.CultureInfo.InvariantCulture, out var width))
        {
            size = widthMatch.Groups[2].Value.Equals("px", StringComparison.OrdinalIgnoreCase)
                ? width * 0.75 : width;
        }
        var colorMatch = Regex.Match(value, @"#([0-9a-f]{3}|[0-9a-f]{6})\b", RegexOptions.IgnoreCase);
        var color = colorMatch.Success ? NormalizarColor(colorMatch.Value) : "000000";
        return (size, color);
    }

    private static string NormalizarColor(string value)
    {
        var color = value.Trim().TrimStart('#');
        if (color.Length == 3) color = string.Concat(color.Select(c => $"{c}{c}"));
        return Regex.IsMatch(color, "^[0-9a-fA-F]{6}$") ? color.ToUpperInvariant() : "000000";
    }

    // ==================== DATA TYPES ====================

    private record CellData(string Text, double Width, Dictionary<string, string> Css, int Colspan = 0);
    private record TableRowData(List<CellData> Cells, bool IsHeader = false);

    // ==================== FONT RESOLVER ====================

    public static HashSet<string> DetectarVariantesFuente(JsonNode json)
    {
        var variantes = new HashSet<string> { "" };
        DetectarVariantesEnNodo(json, variantes);
        return variantes;
    }

    private static void DetectarVariantesEnNodo(JsonNode? node, HashSet<string> variantes)
    {
        if (node is JsonObject obj)
        {
            foreach (var prop in obj)
            {
                var val = prop.Value?.ToString() ?? "";
                if (prop.Key.Contains("style", StringComparison.OrdinalIgnoreCase) ||
                    prop.Key.Contains("Style", StringComparison.Ordinal))
                {
                    if (Regex.IsMatch(val, @"font-weight\s*:\s*(bold|[6-9]\d\d|[1-9]\d{3})", RegexOptions.IgnoreCase))
                        variantes.Add("b");
                    if (Regex.IsMatch(val, @"font-style\s*:\s*(italic|oblique)", RegexOptions.IgnoreCase))
                        variantes.Add("i");
                    if (variantes.Contains("b") && variantes.Contains("i"))
                        variantes.Add("bi");
                }
                DetectarVariantesEnNodo(prop.Value, variantes);
            }
        }
        else if (node is JsonArray arr)
        {
            foreach (var item in arr)
                DetectarVariantesEnNodo(item, variantes);
        }
    }

    public static Dictionary<string, string> ObtenerRutasS3Fuentes(string fontFamily, HashSet<string> variantes)
    {
        var baseName = fontFamily.Split(',')[0].Trim().ToLowerInvariant().Replace(" ", "");
        var rutas = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        foreach (var variante in variantes)
            rutas[baseName + variante] = $"fuentes/{baseName}{variante}.ttf";
        return rutas;
    }

    private class S3FontResolver : IFontResolver
    {
        private readonly Dictionary<string, byte[]> _fonts;

        public S3FontResolver(Dictionary<string, byte[]> fonts)
        {
            _fonts = new Dictionary<string, byte[]>(fonts, StringComparer.OrdinalIgnoreCase);
        }

        public void AgregarFuentes(Dictionary<string, byte[]> fuentes)
        {
            foreach (var (nombre, bytes) in fuentes)
                _fonts[nombre] = bytes;
        }

        public FontResolverInfo? ResolveTypeface(string familyName, bool isBold, bool isItalic)
        {
            var suffix = (isBold, isItalic) switch
            {
                (true, true) => "bi",
                (true, false) => "b",
                (false, true) => "i",
                _ => ""
            };

            var baseName = familyName.ToLowerInvariant().Replace(" ", "");
            var candidates = new[]
            {
                baseName + suffix,
                baseName,
            };

            foreach (var candidate in candidates)
                if (_fonts.ContainsKey(candidate))
                    return new FontResolverInfo(candidate);

            if (_fonts.Count > 0)
                return new FontResolverInfo(_fonts.Keys.First());

            return null;
        }

        public byte[]? GetFont(string faceName)
        {
            return _fonts.TryGetValue(faceName, out var bytes) ? bytes : null;
        }
    }
}
