namespace SafetyReport.Application.Puertos.TablaMaestra;

public interface ITablaMaestraTranslator
{
    Task<TablaMaestraTranslationOutput> TranslateAsync(TablaMaestraTranslationInput input);
}
