namespace TaxStream.Application.DTOs.AI;

public class AiChatResponseDto
{
    public Guid ConversationId { get; set; }
    public string Response { get; set; } = string.Empty;
    public string Disclaimer { get; set; } = "AI responses are for informational purposes only and may be incomplete or outdated. Always verify with official tax guidance or a qualified professional.";
}
