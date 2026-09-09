using SafetyReport.Models;

namespace SafetyReport.Application.Puertos.Informe;

public interface IInformeTranslator
{
    Task<InformeTranslationContent> TranslateAsync(InformeTranslationContent contenido, string idioma);
}
