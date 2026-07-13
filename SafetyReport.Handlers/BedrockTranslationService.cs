using System.Text.Json;

namespace SafetyReport.Handlers
{
    public class TranslationInput
    {
        public string? String1 { get; set; }
        public string? String2 { get; set; }
        public int IdIdioma { get; set; } = 1;
    }

    public class TranslationOutput
    {
        public string? String1 { get; set; }
        public string? String2 { get; set; }
        public string? String4 { get; set; }
        public string? String5 { get; set; }
        public string? String6 { get; set; }
        public string? String7 { get; set; }
    }

    public class BedrockTranslationService
    {
        private readonly BedrockService _bedrock;

        private static BedrockConfig BuildConfig(string modelId, string systemPrompt) => new()
        {
            ModelId = modelId,
            MaxTokens = 1024,
            Temperature = 0.1F,
            SystemPrompt = systemPrompt
        };

        private static readonly string _promptFromSpanish =
            """
            You are a translator. You receive a JSON object with fields "string1" and "string2" (one or both may be null).

            The input values are in Spanish. Translate them into English and Portuguese.

            You MUST respond with ONLY a valid JSON object using EXACTLY these field names:
            {"string4":"...","string5":"...","string6":"...","string7":"..."}

            Where:
            - string4 = English translation of string1
            - string5 = English translation of string2
            - string6 = Portuguese translation of string1
            - string7 = Portuguese translation of string2

            Rules:
            - If string2 is null, omit string5 and string7.
            - Do NOT rename the fields. Do NOT add extra fields.
            - Do NOT wrap in markdown, code blocks, or explanations.
            - Output ONLY the raw JSON object.

            Example input:  {"string1":"Nuevo Sol","string2":null}
            Example output: {"string4":"Peruvian Sol","string6":"Sol Peruano"}

            Example input:  {"string1":"Dólar Americano","string2":"USD"}
            Example output: {"string4":"US Dollar","string5":"USD","string6":"Dólar Americano","string7":"USD"}
            """;

        private static readonly string _promptFromEnglish =
            """
            You are a translator. You receive a JSON object with fields "string1" and "string2" (one or both may be null).

            The input values are in English. Translate them into Spanish and Portuguese.

            You MUST respond with ONLY a valid JSON object using EXACTLY these field names:
            {"string1":"...","string2":"...","string6":"...","string7":"..."}

            Where:
            - string1 = Spanish translation of string1
            - string2 = Spanish translation of string2
            - string6 = Portuguese translation of string1
            - string7 = Portuguese translation of string2

            Rules:
            - If string2 is null, omit string2 and string7.
            - Do NOT rename the fields. Do NOT add extra fields.
            - Do NOT wrap in markdown, code blocks, or explanations.
            - Output ONLY the raw JSON object.

            Example input:  {"string1":"Peruvian Sol","string2":null}
            Example output: {"string1":"Nuevo Sol","string6":"Sol Peruano"}

            Example input:  {"string1":"US Dollar","string2":"USD"}
            Example output: {"string1":"Dólar Americano","string2":"USD","string6":"Dólar Americano","string7":"USD"}
            """;

        private static readonly string _promptFromPortuguese =
            """
            You are a translator. You receive a JSON object with fields "string1" and "string2" (one or both may be null).

            The input values are in Portuguese. Translate them into Spanish and English.

            You MUST respond with ONLY a valid JSON object using EXACTLY these field names:
            {"string1":"...","string2":"...","string4":"...","string5":"..."}

            Where:
            - string1 = Spanish translation of string1
            - string2 = Spanish translation of string2
            - string4 = English translation of string1
            - string5 = English translation of string2

            Rules:
            - If string2 is null, omit string2 and string5.
            - Do NOT rename the fields. Do NOT add extra fields.
            - Do NOT wrap in markdown, code blocks, or explanations.
            - Output ONLY the raw JSON object.

            Example input:  {"string1":"Sol Peruano","string2":null}
            Example output: {"string1":"Nuevo Sol","string4":"Peruvian Sol"}

            Example input:  {"string1":"Dólar Americano","string2":"USD"}
            Example output: {"string1":"Dólar Americano","string2":"USD","string4":"US Dollar","string5":"USD"}
            """;

        private readonly BedrockConfig _configEs;
        private readonly BedrockConfig _configEn;
        private readonly BedrockConfig _configPt;

        public BedrockTranslationService(BedrockService bedrock, string modelId)
        {
            _bedrock = bedrock;
            _configEs = BuildConfig(modelId, _promptFromSpanish);
            _configEn = BuildConfig(modelId, _promptFromEnglish);
            _configPt = BuildConfig(modelId, _promptFromPortuguese);
        }

        public async Task<TranslationOutput> TranslateAsync(TranslationInput input)
        {
            var config = input.IdIdioma switch
            {
                2 => _configEn,
                3 => _configPt,
                _ => _configEs
            };

            var payload = new { string1 = input.String1, string2 = input.String2 };
            var userMessage = JsonSerializer.Serialize(payload);

            var responseText = await _bedrock.InvokeAsync(config, userMessage);

            return ParseResponse(responseText);
        }

        private static TranslationOutput ParseResponse(string text)
        {
            var start = text.IndexOf('{');
            var end = text.LastIndexOf('}');
            if (start >= 0 && end > start)
                text = text[start..(end + 1)];

            try
            {
                var doc = JsonDocument.Parse(text);
                var root = doc.RootElement;
                var output = new TranslationOutput();

                foreach (var prop in root.EnumerateObject())
                {
                    var value = prop.Value.ValueKind == JsonValueKind.String ? prop.Value.GetString() : null;
                    if (value == null) continue;

                    var key = prop.Name.ToLowerInvariant();
                    if (key == "string1")
                        output.String1 ??= value;
                    else if (key == "string2")
                        output.String2 ??= value;
                    else if (key.Contains("string4") || key.Contains("eng") && key.Contains("1") || key == "s4")
                        output.String4 ??= value;
                    else if (key.Contains("string5") || key.Contains("eng") && key.Contains("2") || key == "s5")
                        output.String5 ??= value;
                    else if (key.Contains("string6") || key.Contains("port") && key.Contains("1") || key == "s6")
                        output.String6 ??= value;
                    else if (key.Contains("string7") || key.Contains("port") && key.Contains("2") || key == "s7")
                        output.String7 ??= value;
                }

                if (output.String4 == null && output.String6 == null)
                {
                    var props = root.EnumerateObject().Where(p => p.Value.ValueKind == JsonValueKind.String).ToList();
                    if (props.Count >= 2)
                    {
                        output.String4 = props[0].Value.GetString();
                        output.String6 = props[1].Value.GetString();
                    }
                    if (props.Count >= 4)
                    {
                        output.String5 = props[2].Value.GetString();
                        output.String7 = props[3].Value.GetString();
                    }
                }

                return output;
            }
            catch
            {
                return new TranslationOutput();
            }
        }
    }
}
