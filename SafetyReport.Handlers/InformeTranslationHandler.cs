using SafetyReport.Models;

namespace SafetyReport.Handlers
{
    public class InformeTranslationHandler
    {
        private readonly BedrockInformeTranslationService _translator;

        public InformeTranslationHandler(BedrockInformeTranslationService translator)
        {
            _translator = translator;
        }

        public async Task<Respuesta> TranslateAsync(InformeTranslationRequest request)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(request.Idioma))
                {
                    return new Respuesta
                    {
                        IdTipoMensaje = 1,
                        Mensaje = "El idioma es requerido.",
                        Result = new InformeTranslationContent()
                    };
                }

                var resultado = await _translator.TranslateAsync(request.Contenido, request.Idioma);

                return new Respuesta
                {
                    IdTipoMensaje = 2,
                    Mensaje = "Traducción completada.",
                    Result = resultado
                };
            }
            catch
            {
                return new Respuesta
                {
                    IdTipoMensaje = 1,
                    Mensaje = "La traducción falló, intente nuevamente.",
                    Result = new InformeTranslationContent()
                };
            }
        }
    }
}
