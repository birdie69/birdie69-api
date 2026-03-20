using Birdie69.Domain.Entities;
using Birdie69.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Birdie69.Infrastructure.Persistence.Repositories;

public sealed class StreakRepository(AppDbContext context)
    : Repository<Streak>(context), IStreakRepository
{
    public async Task<Streak?> GetByUserIdAsync(Guid userId, CancellationToken ct = default)
        => await DbSet.FirstOrDefaultAsync(s => s.UserId == userId, ct);
}
