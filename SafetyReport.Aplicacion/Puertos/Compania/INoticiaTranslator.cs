namespace SafetyReport.Application.Puertos.Compania;

public interface INoticiaTranslator
{
    /// <summary>Traduce entre dos idiomas definidos (español, inglés o portugués).</summary>
    Task<CompaniaNoticiaTraducida> TranslateAsync(string? titulo, string? descripcion, int idIdiomaOrigen, int idIdiomaDestino);

    /// <summary>Detecta el idioma de origen (desconocido) y traduce al español, incluida la categoría.</summary>
    Task<CompaniaNoticiaTraducida> TranslateUnknownToSpanishAsync(string? titulo, string? descripcion, string? categoria);
}
