using Birdie69.Domain.Entities;
using Birdie69.Domain.Interfaces;
using Birdie69.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;

namespace Birdie69.Infrastructure.Persistence.Repositories;

public sealed class CoupleRepository(AppDbContext context)
    : Repository<Couple>(context), ICoupleRepository
{
    public async Task<Couple?> GetByInviteCodeAsync(string inviteCode, CancellationToken cancellationToken = default)
    {
        var codeResult = InviteCode.From(inviteCode);
        if (codeResult.IsFailure) return null;

        var code = codeResult.Value;
        return await DbSet.FirstOrDefaultAsync(
            c => c.InviteCode == code && c.Status == CoupleStatus.Pending,
            cancellationToken);
    }

    public async Task<Couple?> GetActiveByUserIdAsync(Guid userId, CancellationToken cancellationToken = default)
        => await DbSet.FirstOrDefaultAsync(
            c => c.Status == CoupleStatus.Active &&
                 (c.InitiatorId == userId || c.PartnerId == userId),
            cancellationToken);

    public async Task<Couple?> GetCurrentCoupleAsync(Guid userId, CancellationToken cancellationToken = default)
        => await DbSet.FirstOrDefaultAsync(
            c => (c.Status == CoupleStatus.Active || c.Status == CoupleStatus.Pending) &&
                 (c.InitiatorId == userId || c.PartnerId == userId),
            cancellationToken);

    public async Task<bool> HasActiveCoupleAsync(Guid userId, CancellationToken cancellationToken = default)
        => await DbSet.AnyAsync(
            c => c.Status == CoupleStatus.Active &&
                 (c.InitiatorId == userId || c.PartnerId == userId),
            cancellationToken);
}
