namespace TaxStream.Core.Entities;

public class AiConversation
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public string UserPrompt { get; set; } = string.Empty;
    public string AiResponse { get; set; } = string.Empty;
    public DateTimeOffset Timestamp { get; set; } = DateTimeOffset.UtcNow;

    // Navigation
    public User? User { get; set; }
}
