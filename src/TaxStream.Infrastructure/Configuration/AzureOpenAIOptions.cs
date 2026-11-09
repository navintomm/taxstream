namespace TaxStream.Infrastructure.Configuration;

public class AzureOpenAIOptions
{
    public const string SectionName = "AzureOpenAI";

    public string Endpoint { get; set; } = string.Empty;
    public string ApiKey { get; set; } = string.Empty;
    public string DeploymentName { get; set; } = string.Empty;
    public int MaxTokens { get; set; } = 800;
    public int TimeoutSeconds { get; set; } = 30;
}
