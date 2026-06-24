using Amazon.BedrockRuntime;
using Amazon.BedrockRuntime.Model;
using System.Text.Json;

namespace SafetyReport.Handlers
{
    public class TranslationInput
    {
        public string? String1 { get; set; }
        public string? String2 { get; set; }
    }

    public class TranslationOutput
    {
        public string? String4 { get; set; }
        public string? String5 { get; set; }
        public string? String6 { get; set; }
        public string? String7 { get; set; }
    }

    public class BedrockTranslationService
    {
        private readonly IAmazonBedrockRuntime _bedrock;
        private const string ModelId = "meta.llama4-maverick-17b-instruct-v1:0";

        private const string SystemPrompt =
            """
            You are a translator. You receive a JSON object with fields "string1" and "string2" (one or both may be null).

            Translate the non-null values into English and Portuguese.

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
            Example output: {"string4":"New Sol","string6":"Novo Sol"}

            Example input:  {"string1":"Dólar Americano","string2":"USD"}
            Example output: {"string4":"US Dollar","string5":"USD","string6":"Dólar Americano","string7":"USD"}
            """;

        public BedrockTranslationService(IAmazonBedrockRuntime bedrock)
        {
            _bedrock = bedrock;
        }

        public async Task<TranslationOutput> TranslateAsync(TranslationInput input)
        {
            var payload = new { string1 = input.String1, string2 = input.String2 };
            var userMessage = JsonSerializer.Serialize(payload);

            var request = new ConverseRequest
            {
                ModelId = ModelId,
                System = new List<SystemContentBlock>
                {
                    new() { Text = SystemPrompt }
                },
                Messages = new List<Message>
                {
                    new()
                    {
                        Role = ConversationRole.User,
                        Content = new List<ContentBlock>
                        {
                            new() { Text = userMessage }
                        }
                    }
                },
                InferenceConfig = new InferenceConfiguration
                {
                    MaxTokens = 256,
                    Temperature = 0F
                }
            };

            var response = await _bedrock.ConverseAsync(request);
            var responseText = (response?.Output?.Message?.Content?[0]?.Text ?? "{}").Trim();

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
                    if (key.Contains("string4") || key.Contains("eng") && key.Contains("1") || key == "s4")
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
