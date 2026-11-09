using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TaxStream.Core.Entities;

namespace TaxStream.Infrastructure.Persistence.Configurations;

public class AiConversationConfiguration : IEntityTypeConfiguration<AiConversation>
{
    public void Configure(EntityTypeBuilder<AiConversation> builder)
    {
        builder.ToTable("AiConversations");
        builder.HasKey(c => c.Id);
        builder.Property(c => c.UserPrompt).IsRequired().HasMaxLength(2000);
        builder.Property(c => c.AiResponse).IsRequired();
        builder.Property(c => c.Timestamp).IsRequired();

        builder.HasIndex(c => new { c.UserId, c.Timestamp });
    }
}
