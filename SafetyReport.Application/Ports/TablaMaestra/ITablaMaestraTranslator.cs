namespace SafetyReport.Application.Ports.TablaMaestra;

public interface ITablaMaestraTranslator
{
    Task<TablaMaestraTranslationOutput> TranslateAsync(TablaMaestraTranslationInput input);
}
