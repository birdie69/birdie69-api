using Birdie69.Domain.Entities;

namespace Birdie69.Domain.Interfaces;

public interface ICoupleRepository : IRepository<Couple>
{
    Task<Couple?> GetByInviteCodeAsync(string inviteCode, CancellationToken cancellationToken = default);
    Task<Couple?> GetActiveByUserIdAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<bool> HasActiveCoupleAsync(Guid userId, CancellationToken cancellationToken = default);
}
