using Birdie69.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Birdie69.Infrastructure.Persistence.Configurations;

public sealed class AnswerConfiguration : IEntityTypeConfiguration<Answer>
{
    public void Configure(EntityTypeBuilder<Answer> builder)
    {
        builder.HasKey(a => a.Id);

        builder.Property(a => a.Text)
            .IsRequired()
            .HasMaxLength(1000);

        builder.HasOne<Question>()
            .WithMany()
            .HasForeignKey(a => a.QuestionId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne<Birdie69.Domain.Entities.User>()
            .WithMany()
            .HasForeignKey(a => a.UserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne<Couple>()
            .WithMany()
            .HasForeignKey(a => a.CoupleId)
            .OnDelete(DeleteBehavior.Restrict);

        // Business rule: one answer per user per question
        builder.HasIndex(a => new { a.UserId, a.QuestionId })
            .IsUnique();

        builder.HasIndex(a => new { a.QuestionId, a.CoupleId });
    }
}
