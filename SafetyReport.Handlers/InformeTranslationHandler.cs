using Microsoft.Extensions.Logging;
using SafetyReport.Models;

namespace SafetyReport.Handlers
{
    public class InformeTranslationHandler
    {
        private readonly BedrockInformeTranslationService _translator;
        private readonly ILogger<InformeTranslationHandler> _logger;

        public InformeTranslationHandler(BedrockInformeTranslationService translator, ILogger<InformeTranslationHandler> logger)
        {
            _translator = translator;
            _logger = logger;
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

                _logger.LogInformation("[InformeTranslation] Iniciando traduccion a {Idioma}", request.Idioma);

                var resultado = await _translator.TranslateAsync(request.Contenido, request.Idioma);

                _logger.LogInformation("[InformeTranslation] Traduccion completada");

                return new Respuesta
                {
                    IdTipoMensaje = 2,
                    Mensaje = "Traducción completada.",
                    Result = resultado
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[InformeTranslation] Error al traducir");
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
