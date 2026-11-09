using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TaxStream.Core.Entities;

namespace TaxStream.Infrastructure.Persistence.Configurations;

public class TaxRecordConfiguration : IEntityTypeConfiguration<TaxRecord>
{
    public void Configure(EntityTypeBuilder<TaxRecord> builder)
    {
        builder.ToTable("TaxRecords");
        builder.HasKey(t => t.Id);
        builder.Property(t => t.TaxYear).IsRequired();
        builder.Property(t => t.GrossIncome).IsRequired().HasPrecision(18, 2);
        builder.Property(t => t.TotalDeductions).IsRequired().HasPrecision(18, 2);
        builder.Property(t => t.TaxableIncome).IsRequired().HasPrecision(18, 2);
        builder.Property(t => t.TaxLiability).IsRequired().HasPrecision(18, 2);
        builder.Property(t => t.FilingStatus).IsRequired().HasMaxLength(50);
        builder.Property(t => t.CreatedAt).IsRequired();
        builder.Property(t => t.UpdatedAt).IsRequired();

        builder.HasIndex(t => new { t.UserId, t.TaxYear });

        builder.HasMany(t => t.Deductions)
               .WithOne(d => d.TaxRecord)
               .HasForeignKey(d => d.TaxRecordId)
               .OnDelete(DeleteBehavior.Cascade);
    }
}
