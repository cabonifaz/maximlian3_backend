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
        private const string ModelId = "us.meta.llama4-maverick-17b-instruct-v1:0";

        private const string SystemPrompt =
            """
            You are a translator. You receive a JSON object with fields "string1" and "string2" (one or both may be null).

            Translate the non-null values into English and Portuguese. Return ONLY a JSON object with these fields:
            - "string4": English translation of string1
            - "string5": English translation of string2
            - "string6": Portuguese translation of string1
            - "string7": Portuguese translation of string2

            Only include fields for non-null inputs. If string2 is null, omit string5 and string7.

            Return ONLY the JSON object, no explanation, no markdown.
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
            var responseText = response?.Output?.Message?.Content?[0]?.Text ?? "{}";

            return JsonSerializer.Deserialize<TranslationOutput>(responseText, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            }) ?? new TranslationOutput();
        }
    }
}
