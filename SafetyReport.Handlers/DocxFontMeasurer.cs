namespace SafetyReport.Handlers;

/// <summary>
/// Measures text width in points from TrueType font bytes already in FontStore.
/// Parses head → unitsPerEm, hhea → numberOfHMetrics, hmtx → advance widths,
/// cmap format 4 → char-to-glyph mapping. No file I/O, no external libraries.
/// </summary>
internal static class DocxFontMeasurer
{
    private static readonly Dictionary<string, FontData?> _cache = new(StringComparer.OrdinalIgnoreCase);

    public static double MeasureString(string text, string family, double sizePt, bool bold, bool italic)
    {
        var key = $"{family}|{bold}|{italic}";
        if (!_cache.TryGetValue(key, out var font))
        {
            var bytes = FontStore.ObtenerBytes(family, bold, italic);
            font = bytes is not null ? ParseFont(bytes) : null;
            _cache[key] = font;
        }

        if (font is null) return 0;

        double total = 0;
        foreach (var c in text)
            total += font.GetAdvanceWidth(c);

        return total / font.UnitsPerEm * sizePt;
    }

    private static FontData? ParseFont(byte[] bytes)
    {
        try
        {
            using var ms = new MemoryStream(bytes);
            using var r = new BinaryReader(ms);
            return ParseTrueType(r);
        }
        catch { return null; }
    }

    private static FontData? ParseTrueType(BinaryReader r)
    {
        ReadUInt32(r); // sfVersion
        var numTables = ReadUInt16(r);
        ReadUInt16(r); ReadUInt16(r); ReadUInt16(r); // searchRange, entrySelector, rangeShift

        var tables = new Dictionary<string, uint>(numTables);
        for (int i = 0; i < numTables; i++)
        {
            var tag = new string(r.ReadChars(4));
            ReadUInt32(r); // checksum
            var offset = ReadUInt32(r);
            ReadUInt32(r); // length
            tables[tag] = offset;
        }

        if (!tables.ContainsKey("head") || !tables.ContainsKey("hhea") ||
            !tables.ContainsKey("hmtx") || !tables.ContainsKey("cmap"))
            return null;

        // head → unitsPerEm at byte 18
        r.BaseStream.Seek(tables["head"] + 18, SeekOrigin.Begin);
        var unitsPerEm = ReadUInt16(r);

        // hhea → numberOfHMetrics at byte 34
        r.BaseStream.Seek(tables["hhea"] + 34, SeekOrigin.Begin);
        var numHMetrics = ReadUInt16(r);

        // hmtx → advance widths
        r.BaseStream.Seek(tables["hmtx"], SeekOrigin.Begin);
        var advanceWidths = new int[numHMetrics];
        for (int i = 0; i < numHMetrics; i++)
        {
            advanceWidths[i] = ReadUInt16(r);
            ReadUInt16(r); // lsb (skip)
        }

        var charToGlyph = ParseCmap(r, tables["cmap"]);
        return new FontData(unitsPerEm, advanceWidths, charToGlyph);
    }

    private static Dictionary<int, int> ParseCmap(BinaryReader r, uint cmapOffset)
    {
        r.BaseStream.Seek(cmapOffset, SeekOrigin.Begin);
        ReadUInt16(r); // version
        var numSubtables = ReadUInt16(r);

        uint bestSubOffset = 0;
        int bestPriority = -1;

        for (int i = 0; i < numSubtables; i++)
        {
            var platformId = ReadUInt16(r);
            var encodingId = ReadUInt16(r);
            var subOffset  = ReadUInt32(r);

            int priority = (platformId, encodingId) switch
            {
                (3, 1)    => 3,
                (0, >= 3) => 2,
                (0, _)    => 1,
                _         => -1
            };

            if (priority > bestPriority)
            {
                bestPriority  = priority;
                bestSubOffset = cmapOffset + subOffset;
            }
        }

        if (bestSubOffset == 0) return [];

        r.BaseStream.Seek(bestSubOffset, SeekOrigin.Begin);
        var format = ReadUInt16(r);
        return format == 4 ? ParseCmapFormat4(r, bestSubOffset) : [];
    }

    private static Dictionary<int, int> ParseCmapFormat4(BinaryReader r, uint tableStart)
    {
        r.BaseStream.Seek(tableStart + 2, SeekOrigin.Begin);
        var length   = ReadUInt16(r);
        ReadUInt16(r); // language
        var segCount = ReadUInt16(r) / 2;
        ReadUInt16(r); ReadUInt16(r); ReadUInt16(r); // searchRange, entrySelector, rangeShift

        var endCodes   = new int[segCount]; for (int i = 0; i < segCount; i++) endCodes[i]   = ReadUInt16(r);
        ReadUInt16(r); // reservedPad
        var startCodes = new int[segCount]; for (int i = 0; i < segCount; i++) startCodes[i] = ReadUInt16(r);
        var idDeltas   = new int[segCount]; for (int i = 0; i < segCount; i++) idDeltas[i]   = ReadInt16(r);

        var idRangeOffsetBase = r.BaseStream.Position;
        var idRangeOffsets    = new int[segCount]; for (int i = 0; i < segCount; i++) idRangeOffsets[i] = ReadUInt16(r);

        var glyphIdArrayStart = r.BaseStream.Position;
        var glyphCount        = (int)((tableStart + length - glyphIdArrayStart) / 2);
        var glyphIdArray      = new int[glyphCount]; for (int i = 0; i < glyphCount; i++) glyphIdArray[i] = ReadUInt16(r);

        var result = new Dictionary<int, int>();

        for (int seg = 0; seg < segCount - 1; seg++)
        {
            for (int c = startCodes[seg]; c <= endCodes[seg]; c++)
            {
                int glyphId;
                if (idRangeOffsets[seg] == 0)
                {
                    glyphId = (c + idDeltas[seg]) & 0xFFFF;
                }
                else
                {
                    var entryPos     = idRangeOffsetBase + seg * 2;
                    var glyphBytePos = entryPos + idRangeOffsets[seg] + (c - startCodes[seg]) * 2;
                    var arrayIndex   = (int)((glyphBytePos - glyphIdArrayStart) / 2);
                    if (arrayIndex < 0 || arrayIndex >= glyphIdArray.Length) continue;
                    glyphId = glyphIdArray[arrayIndex];
                    if (glyphId != 0) glyphId = (glyphId + idDeltas[seg]) & 0xFFFF;
                }

                if (glyphId != 0) result[c] = glyphId;
            }
        }

        return result;
    }

    private static uint ReadUInt32(BinaryReader r) { var b = r.ReadBytes(4); return (uint)(b[0] << 24 | b[1] << 16 | b[2] << 8 | b[3]); }
    private static int  ReadUInt16(BinaryReader r) { var b = r.ReadBytes(2); return b[0] << 8 | b[1]; }
    private static int  ReadInt16(BinaryReader r)  { var v = ReadUInt16(r); return v >= 0x8000 ? v - 0x10000 : v; }

    private sealed class FontData(int unitsPerEm, int[] advanceWidths, Dictionary<int, int> charToGlyph)
    {
        public int UnitsPerEm { get; } = unitsPerEm;

        public int GetAdvanceWidth(char c)
        {
            charToGlyph.TryGetValue(c, out var glyphId);
            return advanceWidths[Math.Min(glyphId, advanceWidths.Length - 1)];
        }
    }
}
