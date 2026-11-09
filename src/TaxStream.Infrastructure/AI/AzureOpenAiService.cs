using Azure;
using Azure.AI.OpenAI;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using OpenAI.Chat;
using TaxStream.Application.Abstractions;
using TaxStream.Infrastructure.Configuration;

namespace TaxStream.Infrastructure.AI;

public class AzureOpenAiService : IAiProviderService
{
    private readonly AzureOpenAIOptions _options;
    private readonly ILogger<AzureOpenAiService> _logger;
    private AzureOpenAIClient? _client;

    public AzureOpenAiService(IOptions<AzureOpenAIOptions> options, ILogger<AzureOpenAiService> logger)
    {
        _options = options.Value;
        _logger = logger;
    }

    private AzureOpenAIClient GetClient()
    {
        if (_client != null) return _client;
        if (string.IsNullOrWhiteSpace(_options.Endpoint) || _options.Endpoint.StartsWith("REPLACE_")
            || string.IsNullOrWhiteSpace(_options.ApiKey) || _options.ApiKey.StartsWith("REPLACE_"))
        {
            throw new InvalidOperationException(
                "Azure OpenAI is not configured. Please set AzureOpenAI:Endpoint and AzureOpenAI:ApiKey via user-secrets.");
        }

        _client = new AzureOpenAIClient(
            new Uri(_options.Endpoint),
            new AzureKeyCredential(_options.ApiKey));
        return _client;
    }

    public async Task<string> SendAsync(string systemPrompt, string userMessage, CancellationToken cancellationToken = default)
    {
        using var cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        cts.CancelAfter(TimeSpan.FromSeconds(_options.TimeoutSeconds));

        var chatClient = GetClient().GetChatClient(_options.DeploymentName);

        var messages = new List<ChatMessage>
        {
            new SystemChatMessage(systemPrompt),
            new UserChatMessage(userMessage)
        };

        var options = new ChatCompletionOptions
        {
            MaxOutputTokenCount = _options.MaxTokens
        };

        var response = await chatClient.CompleteChatAsync(messages, options, cts.Token);
        var content = response.Value.Content[0].Text;

        _logger.LogInformation("AI provider responded. DeploymentName: {Deployment}", _options.DeploymentName);
        return content;
    }
}
