namespace TaxStream.Application.Abstractions;

public interface IAiProviderService
{
    Task<string> SendAsync(string systemPrompt, string userMessage, CancellationToken cancellationToken = default);
}
