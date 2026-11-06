using System.Globalization;

namespace SafetyReport.Application.Comun;

/// <summary>Validación de zonas horarias y resolución de su desplazamiento UTC.</summary>
public static class FechaZonaHoraria
{
    public static bool EsValida(string? timezone) =>
        !string.IsNullOrWhiteSpace(timezone)
        && TimeZoneInfo.TryFindSystemTimeZoneById(timezone, out _);

    // El desplazamiento corresponde al instante indicado; puede variar por horario de verano.
    public static string ObtenerOffset(string timezone, DateTimeOffset instanteUtc)
    {
        var zona = TimeZoneInfo.FindSystemTimeZoneById(timezone);
        var offset = zona.GetUtcOffset(instanteUtc);
        var signo = offset < TimeSpan.Zero ? "-" : "+";
        return signo + offset.Duration().ToString(@"hh\:mm", CultureInfo.InvariantCulture);
    }
}
