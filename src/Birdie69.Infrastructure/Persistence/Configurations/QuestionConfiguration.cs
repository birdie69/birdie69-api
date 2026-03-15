using Birdie69.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Birdie69.Infrastructure.Persistence.Configurations;

public sealed class QuestionConfiguration : IEntityTypeConfiguration<Question>
{
    public void Configure(EntityTypeBuilder<Question> builder)
    {
        builder.HasKey(q => q.Id);

        builder.Property(q => q.ExternalDocumentId)
            .IsRequired()
            .HasMaxLength(128);

        builder.HasIndex(q => q.ExternalDocumentId).IsUnique();
        builder.HasIndex(q => q.ScheduledDate).IsUnique();

        builder.Property(q => q.Title)
            .IsRequired()
            .HasMaxLength(500);

        builder.Property(q => q.Body)
            .IsRequired()
            .HasMaxLength(2000);

        builder.Property(q => q.Category)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(q => q.Tags)
            .HasConversion(
                v => string.Join(',', v),
                v => (IReadOnlyList<string>)v.Split(',', StringSplitOptions.RemoveEmptyEntries).ToList())
            .HasMaxLength(500)
            .Metadata.SetValueComparer(new ValueComparer<IReadOnlyList<string>>(
                (a, b) => a != null && b != null && a.SequenceEqual(b),
                v => v.Aggregate(0, (h, s) => HashCode.Combine(h, s.GetHashCode())),
                v => v.ToList()));

        builder.Property(q => q.ScheduledDate)
            .HasColumnType("date");
    }
}
