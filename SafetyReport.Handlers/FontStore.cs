namespace SafetyReport.Handlers;

internal static class FontStore
{
    private static readonly Dictionary<string, byte[]> _fonts = new(StringComparer.OrdinalIgnoreCase);
    private static readonly object _lock = new();

    public static void Configurar(Dictionary<string, byte[]> fuentes)
    {
        lock (_lock)
        {
            foreach (var (nombre, bytes) in fuentes)
                _fonts[nombre] = bytes;
        }
    }

    public static byte[]? ObtenerBytes(string family, bool bold, bool italic)
    {
        var baseName = family.ToLowerInvariant().Replace(" ", "");
        var suffix = (bold, italic) switch
        {
            (true, true)  => "bi",
            (true, false) => "b",
            (false, true) => "i",
            _             => ""
        };

        lock (_lock)
        {
            if (_fonts.TryGetValue(baseName + suffix, out var bytes)) return bytes;
            if (_fonts.TryGetValue(baseName, out bytes)) return bytes;
            return null;
        }
    }
}
