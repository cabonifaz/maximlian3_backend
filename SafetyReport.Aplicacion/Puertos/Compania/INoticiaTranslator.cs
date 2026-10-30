namespace SafetyReport.Application.Puertos.Compania;

public interface INoticiaTranslator
{
    Task<CompaniaNoticiaTraducida> TranslateAsync(string? titulo, string? descripcion, int idIdiomaDestino);
}
