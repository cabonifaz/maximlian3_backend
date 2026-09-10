namespace SafetyReport.Domain.Informes;

public static class InformeImagenReglas
{
    private static readonly string[] ExtensionesPermitidas =
    [
        ".jpg",
        ".jpeg",
        ".png",
        ".gif",
        ".webp",
        ".bmp",
        ".tiff"
    ];

    private static readonly HashSet<string> ExtensionesPermitidasLookup =
        new(ExtensionesPermitidas, StringComparer.OrdinalIgnoreCase);

    public static string MensajeNombreRequerido => "El nombre del archivo de imagen es requerido.";

    public static bool EsExtensionPermitida(string nombreArchivo)
    {
        var extension = Path.GetExtension(nombreArchivo);

        return ExtensionesPermitidasLookup.Contains(extension);
    }

    public static string MensajeExtensionNoPermitida(string nombreArchivo)
    {
        return $"El archivo '{nombreArchivo}' no es una imagen válida. Extensiones permitidas: {string.Join(", ", ExtensionesPermitidas)}.";
    }
}
