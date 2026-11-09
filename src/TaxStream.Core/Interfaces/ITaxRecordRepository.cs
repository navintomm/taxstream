using TaxStream.Core.Entities;

namespace TaxStream.Core.Interfaces;

public interface ITaxRecordRepository
{
    Task<TaxRecord?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<TaxRecord?> GetByIdWithDeductionsAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<TaxRecord>> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<TaxRecord> AddAsync(TaxRecord taxRecord, CancellationToken cancellationToken = default);
    Task UpdateAsync(TaxRecord taxRecord, CancellationToken cancellationToken = default);
    Task DeleteAsync(TaxRecord taxRecord, CancellationToken cancellationToken = default);
    Task SaveChangesAsync(CancellationToken cancellationToken = default);
}
