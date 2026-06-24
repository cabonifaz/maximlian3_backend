using Microsoft.Extensions.Logging;
using SafetyReport.Models;
using System.Text.Json;

namespace SafetyReport.Handlers
{
    public class BedrockInformeTranslationService
    {
        private readonly BedrockService _bedrock;
        private readonly string _modelId;
        private readonly ILogger<BedrockInformeTranslationService> _logger;

        private static readonly JsonSerializerOptions _jsonOptions = new()
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            PropertyNameCaseInsensitive = true
        };

        private const string SystemPromptTemplate =
            """
            You are a translator. You receive a JSON object with Spanish text organized in sections.
            Each section is an object with field names as keys and Spanish text as values.

            You MUST translate ALL string values from Spanish to {0}.

            You MUST return the EXACT same JSON structure with translated values. Rules:
            - Do NOT change keys or structure, only translate the string values.
            - If a value is null, keep it as null. If a value is "", keep it as "".
            - Do NOT add, remove, or rename any fields.
            - Do NOT wrap in markdown, code blocks, or explanations.
            - Output ONLY the raw JSON object.

            The structure is:
            {{
              "identificacion": {{ "fieldName": "translated text", ... }},
              "legales": {{ "fieldName": "translated text", ... }},
              "ramoOperaciones": {{
                "campos": {{ "fieldName": "translated text", ... }},
                "importaciones": {{ "fieldName": "translated text", ... }},
                "exportaciones": {{ "fieldName": "translated text", ... }}
              }},
              "informacionFinanciera": {{ "fieldName": "translated text", ... }},
              "bancosProveedores": {{ "fieldName": "translated text", ... }},
              "datosGenerales": {{ "fieldName": "translated text", ... }}
            }}

            Preserve all keys exactly as received. Only translate the values.
            """;

        public BedrockInformeTranslationService(BedrockService bedrock, string modelId, ILogger<BedrockInformeTranslationService> logger)
        {
            _bedrock = bedrock;
            _modelId = modelId;
            _logger = logger;
        }

        public async Task<InformeTranslationContent> TranslateAsync(InformeTranslationContent contenido, string idioma)
        {
            var config = new BedrockConfig
            {
                ModelId = _modelId,
                MaxTokens = 4096,
                Temperature = 0.1F,
                SystemPrompt = string.Format(SystemPromptTemplate, idioma)
            };

            var userMessage = JsonSerializer.Serialize(contenido, _jsonOptions);

            _logger.LogInformation("[BedrockInforme] Enviando a modelo={Model}, idioma={Idioma}, inputLength={Length}", _modelId, idioma, userMessage.Length);

            var responseText = await _bedrock.InvokeAsync(config, userMessage);

            _logger.LogInformation("[BedrockInforme] Respuesta recibida, length={Length}, preview={Preview}", responseText.Length, responseText.Length > 200 ? responseText[..200] : responseText);

            return ParseResponse(responseText, contenido);
        }

        private static InformeTranslationContent ParseResponse(string text, InformeTranslationContent original)
        {
            var start = text.IndexOf('{');
            var end = text.LastIndexOf('}');
            if (start >= 0 && end > start)
                text = text[start..(end + 1)];

            InformeTranslationContent parsed;
            try
            {
                parsed = JsonSerializer.Deserialize<InformeTranslationContent>(text, _jsonOptions) ?? original;
            }
            catch
            {
                return original;
            }

            return new InformeTranslationContent
            {
                Identificacion = EnforceKeys(original.Identificacion, parsed.Identificacion),
                Legales = EnforceKeys(original.Legales, parsed.Legales),
                RamoOperaciones = new InformeTranslationRamoOperaciones
                {
                    Campos = EnforceKeys(original.RamoOperaciones.Campos, parsed.RamoOperaciones.Campos),
                    Importaciones = EnforceKeys(original.RamoOperaciones.Importaciones, parsed.RamoOperaciones.Importaciones),
                    Exportaciones = EnforceKeys(original.RamoOperaciones.Exportaciones, parsed.RamoOperaciones.Exportaciones)
                },
                InformacionFinanciera = EnforceKeys(original.InformacionFinanciera, parsed.InformacionFinanciera),
                BancosProveedores = EnforceKeys(original.BancosProveedores, parsed.BancosProveedores),
                DatosGenerales = EnforceKeys(original.DatosGenerales, parsed.DatosGenerales)
            };
        }

        private static Dictionary<string, string?> EnforceKeys(Dictionary<string, string?> original, Dictionary<string, string?> parsed)
        {
            var result = new Dictionary<string, string?>();
            foreach (var kvp in original)
            {
                if (string.IsNullOrEmpty(kvp.Value))
                    result[kvp.Key] = kvp.Value;
                else if (parsed.TryGetValue(kvp.Key, out var translated) && !string.IsNullOrEmpty(translated))
                    result[kvp.Key] = translated;
                else
                    result[kvp.Key] = kvp.Value;
            }
            return result;
        }
    }
}
