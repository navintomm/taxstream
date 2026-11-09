using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TaxStream.Core.Entities;

namespace TaxStream.Infrastructure.Persistence.Configurations;

public class DeductionConfiguration : IEntityTypeConfiguration<Deduction>
{
    public void Configure(EntityTypeBuilder<Deduction> builder)
    {
        builder.ToTable("Deductions");
        builder.HasKey(d => d.Id);
        builder.Property(d => d.Category).IsRequired().HasMaxLength(100);
        builder.Property(d => d.Description).HasMaxLength(500);
        builder.Property(d => d.Amount).IsRequired().HasPrecision(18, 2);

        builder.HasIndex(d => d.TaxRecordId);
    }
}
