using TaxStream.Core.Entities;

namespace TaxStream.Core.Interfaces;

public interface IAiConversationRepository
{
    Task<AiConversation> AddAsync(AiConversation conversation, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<AiConversation>> GetByUserIdAsync(Guid userId, int pageNumber, int pageSize, CancellationToken cancellationToken = default);
    Task SaveChangesAsync(CancellationToken cancellationToken = default);
}
