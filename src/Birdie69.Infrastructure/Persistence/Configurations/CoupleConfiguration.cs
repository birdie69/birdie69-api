using Birdie69.Domain.Entities;
using Birdie69.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Birdie69.Infrastructure.Persistence.Configurations;

public sealed class CoupleConfiguration : IEntityTypeConfiguration<Couple>
{
    public void Configure(EntityTypeBuilder<Couple> builder)
    {
        builder.HasKey(c => c.Id);

        builder.Property(c => c.Status)
            .HasConversion<string>();

        builder.Property(c => c.InviteCode)
            .HasConversion(
                v => v.Value,
                v => InviteCode.From(v).Value)
            .HasMaxLength(8)
            .IsRequired();

        builder.HasIndex(c => c.InviteCode)
            .IsUnique();

        builder.HasIndex(c => c.InitiatorId);
        builder.HasIndex(c => c.PartnerId);

        // FK → Users: enforce referential integrity
        builder.HasOne<User>()
            .WithMany()
            .HasForeignKey(c => c.InitiatorId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne<User>()
            .WithMany()
            .HasForeignKey(c => c.PartnerId)
            .IsRequired(false)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
