using System.Globalization;
using System.Text;

namespace SafetyReport.Domain.Archivos;

public static class NombreArchivoReglas
{
    public static string Sanitizar(string valor, string valorPorDefecto)
    {
        var sinDiacriticos = EliminarDiacriticos(valor);
        var caracteresInvalidos = Path.GetInvalidFileNameChars();
        var limpio = new string(sinDiacriticos.Where(c => !caracteresInvalidos.Contains(c)).ToArray()).Trim();

        return string.IsNullOrWhiteSpace(limpio) ? valorPorDefecto : limpio;
    }

    public static string SanitizarSegmentoRuta(string valor, string valorPorDefecto)
    {
        var limpio = string.Concat(valor.Where(c => char.IsLetterOrDigit(c) || c == '_' || c == '-'));

        return string.IsNullOrWhiteSpace(limpio) ? valorPorDefecto : limpio;
    }

    private static string EliminarDiacriticos(string valor)
    {
        var sinAcentoSuelto = valor.Replace("´", "").Replace("`", "");
        var normalizado = sinAcentoSuelto.Normalize(NormalizationForm.FormD);
        var sinMarcas = normalizado.Where(c => CharUnicodeInfo.GetUnicodeCategory(c) != UnicodeCategory.NonSpacingMark);

        return new string(sinMarcas.ToArray()).Normalize(NormalizationForm.FormC);
    }
}
