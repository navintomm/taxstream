using Microsoft.EntityFrameworkCore;
using TaxStream.Core.Entities;
using TaxStream.Core.Interfaces;

namespace TaxStream.Infrastructure.Persistence.Repositories;

public class TaxRecordRepository : ITaxRecordRepository
{
    private readonly AppDbContext _context;

    public TaxRecordRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<TaxRecord?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        => await _context.TaxRecords.FindAsync(new object[] { id }, cancellationToken);

    public async Task<TaxRecord?> GetByIdWithDeductionsAsync(Guid id, CancellationToken cancellationToken = default)
        => await _context.TaxRecords
            .Include(t => t.Deductions)
            .FirstOrDefaultAsync(t => t.Id == id, cancellationToken);

    public async Task<IReadOnlyList<TaxRecord>> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default)
        => await _context.TaxRecords
            .Include(t => t.Deductions)
            .Where(t => t.UserId == userId)
            .OrderByDescending(t => t.TaxYear)
            .ToListAsync(cancellationToken);

    public async Task<TaxRecord> AddAsync(TaxRecord taxRecord, CancellationToken cancellationToken = default)
    {
        await _context.TaxRecords.AddAsync(taxRecord, cancellationToken);
        return taxRecord;
    }

    public Task UpdateAsync(TaxRecord taxRecord, CancellationToken cancellationToken = default)
    {
        _context.TaxRecords.Update(taxRecord);
        return Task.CompletedTask;
    }

    public Task DeleteAsync(TaxRecord taxRecord, CancellationToken cancellationToken = default)
    {
        _context.TaxRecords.Remove(taxRecord);
        return Task.CompletedTask;
    }

    public async Task SaveChangesAsync(CancellationToken cancellationToken = default)
        => await _context.SaveChangesAsync(cancellationToken);
}
