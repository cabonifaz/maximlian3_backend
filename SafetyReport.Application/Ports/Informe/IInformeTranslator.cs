using SafetyReport.Models;

namespace SafetyReport.Application.Ports.Informe;

public interface IInformeTranslator
{
    Task<InformeTranslationContent> TranslateAsync(InformeTranslationContent contenido, string idioma);
}
