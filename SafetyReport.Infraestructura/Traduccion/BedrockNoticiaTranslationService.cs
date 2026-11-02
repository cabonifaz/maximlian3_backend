using System.Collections.Concurrent;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using SafetyReport.Application.Puertos.Compania;

namespace SafetyReport.Infrastructure.Traduccion
{
    public class BedrockNoticiaTranslationService : INoticiaTranslator
    {
        private const int MaxEntradasCache = 2000;

        private readonly BedrockService _bedrock;
        private readonly string _modelId;
        private readonly ConcurrentDictionary<string, CompaniaNoticiaTraducida> _cache = new();

        private const string SystemPromptTemplate =
            """
            You are a professional translator. You receive a JSON object with the fields "titulo", "descripcion" and "categoria" of a news item (any of them may be null).

            The values can be written in ANY language (for example Chinese, Japanese, Korean, Russian or Arabic). The source language is detected automatically. Translate every value into {0}.

            Rules:
            - Every non-empty value in your answer must be written in {0}. Never answer in the source language.
            - Return a value unchanged ONLY if it is already written in {0}.
            - If a value is null or "", keep it as is.
            - Keep proper names, company names, figures and dates unchanged.
            - Preserve line breaks.
            - Respond with ONLY a valid JSON object with EXACTLY the fields "titulo", "descripcion" and "categoria".
            - Do NOT wrap in markdown, code blocks, or explanations.
            """;

        public BedrockNoticiaTranslationService(BedrockService bedrock, string modelId)
        {
            _bedrock = bedrock;
            _modelId = modelId;
        }

        public async Task<CompaniaNoticiaTraducida> TranslateAsync(string? titulo, string? descripcion, int idIdiomaDestino, string? categoria = null)
        {
            var clave = $"{idIdiomaDestino}:{Hash(titulo, descripcion, categoria)}";
            if (_cache.TryGetValue(clave, out var enCache))
                return enCache;

            var config = new BedrockConfig
            {
                ModelId = _modelId,
                MaxTokens = 4096,
                Temperature = 0.1F,
                SystemPrompt = string.Format(SystemPromptTemplate, NombreIdioma(idIdiomaDestino))
            };

            var userMessage = JsonSerializer.Serialize(new { titulo, descripcion, categoria });
            var responseText = await _bedrock.InvokeAsync(config, userMessage);
            var traduccion = ParseResponse(responseText);

            // Un resultado idéntico al original no se cachea: si el modelo no tradujo, un reintento debe volver a consultarlo.
            var devolvioSinCambios = traduccion.Titulo == titulo && traduccion.Descripcion == descripcion;
            if (devolvioSinCambios)
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

        private static string Hash(string? titulo, string? descripcion, string? categoria)
        {
            var bytes = SHA256.HashData(Encoding.UTF8.GetBytes($"{titulo}\0{descripcion}\0{categoria}"));
            return Convert.ToHexString(bytes);
        }

        private static CompaniaNoticiaTraducida ParseResponse(string text)
        {
            var start = text.IndexOf('{');
            var end = text.LastIndexOf('}');
            if (start >= 0 && end > start)
                text = text[start..(end + 1)];

            using var doc = JsonDocument.Parse(text);
            var root = doc.RootElement;

            return new CompaniaNoticiaTraducida
            {
                Titulo = LeerTexto(root, "titulo"),
                Descripcion = LeerTexto(root, "descripcion"),
                Categoria = LeerTexto(root, "categoria")
            };
        }

        private static string? LeerTexto(JsonElement root, string campo) =>
            root.TryGetProperty(campo, out var valor) && valor.ValueKind == JsonValueKind.String
                ? valor.GetString()
                : null;
    }
}
