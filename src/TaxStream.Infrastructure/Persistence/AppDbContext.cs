using Microsoft.EntityFrameworkCore;
using TaxStream.Core.Entities;
using TaxStream.Infrastructure.Persistence.Configurations;

namespace TaxStream.Infrastructure.Persistence;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<User> Users => Set<User>();
    public DbSet<TaxRecord> TaxRecords => Set<TaxRecord>();
    public DbSet<Deduction> Deductions => Set<Deduction>();
    public DbSet<AiConversation> AiConversations => Set<AiConversation>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfiguration(new UserConfiguration());
        modelBuilder.ApplyConfiguration(new TaxRecordConfiguration());
        modelBuilder.ApplyConfiguration(new DeductionConfiguration());
        modelBuilder.ApplyConfiguration(new AiConversationConfiguration());
        base.OnModelCreating(modelBuilder);
    }
}
