using System.Collections.Concurrent;
using System.Security.Cryptography;
using System.Text;
using SafetyReport.Application.Puertos.Compania;

namespace SafetyReport.Infrastructure.Traduccion
{
    public class BedrockNoticiaTranslationService : INoticiaTranslator
    {
        private const int MaxEntradasCache = 2000;
        private const int IdIdiomaEspanol = 1;

        private readonly BedrockService _bedrock;
        private readonly string _modelId;
        private readonly ConcurrentDictionary<string, string> _cache = new();

        private const string PromptIdiomaDefinidoTemplate =
            """
            You are a professional translator. Translate the text sent by the user from {0} to {1}.

            The text is written in {0}.

            Rules:
            - The answer must be written in {1}.
            - Keep proper names, company names, figures and dates unchanged.
            - Preserve line breaks.
            - Respond with ONLY the translated text. Do NOT add quotes, markdown, notes or explanations.
            """;

        private const string PromptIdiomaDesconocidoTemplate =
            """
            You are a professional translator. Translate the text sent by the user into {0}.

            The text can be written in ANY language (for example Chinese, Japanese, Korean, Russian, Arabic or Indonesian). The source language is detected automatically.

            Rules:
            - The answer must be written in {0}. Never answer in the source language.
            - Return the text unchanged ONLY if it is already written in {0}.
            - Keep proper names, company names, figures and dates unchanged.
            - Preserve line breaks.
            - Respond with ONLY the translated text. Do NOT add quotes, markdown, notes or explanations.
            """;

        public BedrockNoticiaTranslationService(BedrockService bedrock, string modelId)
        {
            _bedrock = bedrock;
            _modelId = modelId;
        }

        public async Task<CompaniaNoticiaTraducida> TranslateAsync(string? titulo, string? descripcion, int idIdiomaOrigen, int idIdiomaDestino)
        {
            var prompt = string.Format(PromptIdiomaDefinidoTemplate, NombreIdioma(idIdiomaOrigen), NombreIdioma(idIdiomaDestino));
            var tipo = $"definido:{idIdiomaOrigen}:{idIdiomaDestino}";

            var tituloTask = TraducirTextoAsync(titulo, prompt, tipo);
            var descripcionTask = TraducirTextoAsync(descripcion, prompt, tipo);

            await Task.WhenAll(tituloTask, descripcionTask);

            return new CompaniaNoticiaTraducida
            {
                Titulo = tituloTask.Result,
                Descripcion = descripcionTask.Result
            };
        }

        // Cada campo se traduce por separado: si se enviaran juntos, un campo ya en español
        // puede llevar al modelo a devolver todo sin traducir.
        public async Task<CompaniaNoticiaTraducida> TranslateUnknownToSpanishAsync(string? titulo, string? descripcion, string? categoria)
        {
            var prompt = string.Format(PromptIdiomaDesconocidoTemplate, NombreIdioma(IdIdiomaEspanol));
            var tipo = $"desconocido:{IdIdiomaEspanol}";

            var tituloTask = TraducirTextoAsync(titulo, prompt, tipo);
            var descripcionTask = TraducirTextoAsync(descripcion, prompt, tipo);
            var categoriaTask = TraducirTextoAsync(categoria, prompt, tipo);

            await Task.WhenAll(tituloTask, descripcionTask, categoriaTask);

            return new CompaniaNoticiaTraducida
            {
                Titulo = tituloTask.Result,
                Descripcion = descripcionTask.Result,
                Categoria = categoriaTask.Result
            };
        }

        private async Task<string?> TraducirTextoAsync(string? texto, string systemPrompt, string tipo)
        {
            if (string.IsNullOrWhiteSpace(texto))
                return texto;

            var clave = $"{tipo}:{Hash(texto)}";
            if (_cache.TryGetValue(clave, out var enCache))
                return enCache;

            var config = new BedrockConfig
            {
                ModelId = _modelId,
                MaxTokens = 4096,
                Temperature = 0.1F,
                SystemPrompt = systemPrompt
            };

            var traduccion = await _bedrock.InvokeAsync(config, texto);

            // Un resultado idéntico al original no se cachea: si el modelo no tradujo, un reintento debe volver a consultarlo.
            if (traduccion == texto.Trim())
                return traduccion;

            if (_cache.Count >= MaxEntradasCache)
                _cache.Clear();
            _cache[clave] = traduccion;

            return traduccion;
        }

        private static string NombreIdioma(int idIdioma) => idIdioma switch
        {
            2 => "English",
            3 => "Portuguese",
            _ => "Spanish"
        };

        private static string Hash(string texto)
        {
            var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(texto));
            return Convert.ToHexString(bytes);
        }
    }
}
