namespace TaxStream.Application.DTOs.AI;

public class AiHistoryItemDto
{
    public Guid Id { get; set; }
    public string UserPrompt { get; set; } = string.Empty;
    public string AiResponse { get; set; } = string.Empty;
    public DateTimeOffset Timestamp { get; set; }
}
