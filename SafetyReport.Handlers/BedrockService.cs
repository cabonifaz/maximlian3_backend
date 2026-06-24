using Amazon.BedrockRuntime;
using Amazon.BedrockRuntime.Model;

namespace SafetyReport.Handlers
{
    public class BedrockConfig
    {
        public string ModelId { get; set; } = "amazon.nova-lite-v1:0";
        public string SystemPrompt { get; set; } = "";
        public int MaxTokens { get; set; } = 256;
        public float Temperature { get; set; } = 0F;
    }

    public class BedrockService
    {
        private readonly IAmazonBedrockRuntime _bedrock;

        public BedrockService(IAmazonBedrockRuntime bedrock)
        {
            _bedrock = bedrock;
        }

        public async Task<string> InvokeAsync(BedrockConfig config, string userMessage)
        {
            var request = new ConverseRequest
            {
                ModelId = config.ModelId,
                System = new List<SystemContentBlock>
                {
                    new() { Text = config.SystemPrompt }
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
                    MaxTokens = config.MaxTokens,
                    Temperature = config.Temperature
                }
            };

            var response = await _bedrock.ConverseAsync(request);
            return (response?.Output?.Message?.Content?[0]?.Text ?? "").Trim();
        }
    }
}
