using Birdie69.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Birdie69.Infrastructure.Persistence.Configurations;

public sealed class QuestionConfiguration : IEntityTypeConfiguration<Question>
{
    public void Configure(EntityTypeBuilder<Question> builder)
    {
        builder.HasKey(q => q.Id);

        builder.Property(q => q.ExternalId)
            .IsRequired()
            .HasMaxLength(128);

        builder.HasIndex(q => q.ExternalId).IsUnique();
        builder.HasIndex(q => q.ScheduledDate).IsUnique();

        builder.Property(q => q.Text)
            .IsRequired()
            .HasMaxLength(2000);

        builder.Property(q => q.Tags)
            .HasConversion(
                v => string.Join(',', v),
                v => v.Split(',', StringSplitOptions.RemoveEmptyEntries).ToList())
            .HasMaxLength(500);
    }
}
