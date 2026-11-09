using Microsoft.EntityFrameworkCore;
using TaxStream.Core.Entities;
using TaxStream.Core.Interfaces;

namespace TaxStream.Infrastructure.Persistence.Repositories;

public class AiConversationRepository : IAiConversationRepository
{
    private readonly AppDbContext _context;

    public AiConversationRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<AiConversation> AddAsync(AiConversation conversation, CancellationToken cancellationToken = default)
    {
        await _context.AiConversations.AddAsync(conversation, cancellationToken);
        return conversation;
    }

    public async Task<IReadOnlyList<AiConversation>> GetByUserIdAsync(Guid userId, int pageNumber, int pageSize, CancellationToken cancellationToken = default)
        => await _context.AiConversations
            .Where(c => c.UserId == userId)
            .OrderByDescending(c => c.Timestamp)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

    public async Task SaveChangesAsync(CancellationToken cancellationToken = default)
        => await _context.SaveChangesAsync(cancellationToken);
}
