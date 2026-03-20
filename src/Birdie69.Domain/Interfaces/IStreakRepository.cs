using Birdie69.Domain.Entities;

namespace Birdie69.Domain.Interfaces;

public interface IStreakRepository : IRepository<Streak>
{
    Task<Streak?> GetByUserIdAsync(Guid userId, CancellationToken ct = default);
}
