namespace TaxStream.Core.Entities;

public class User
{
    public Guid Id { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string NormalizedEmail { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;

    // Navigation
    public ICollection<TaxRecord> TaxRecords { get; set; } = new List<TaxRecord>();
    public ICollection<AiConversation> AiConversations { get; set; } = new List<AiConversation>();
}
