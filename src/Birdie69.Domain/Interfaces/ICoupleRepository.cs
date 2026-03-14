using Birdie69.Domain.Entities;

namespace Birdie69.Domain.Interfaces;

public interface ICoupleRepository : IRepository<Couple>
{
    Task<Couple?> GetByInviteCodeAsync(string inviteCode, CancellationToken cancellationToken = default);

    /// <summary>Returns the user's Active couple (used for reads/display).</summary>
    Task<Couple?> GetActiveByUserIdAsync(Guid userId, CancellationToken cancellationToken = default);

    /// <summary>Returns the user's Active OR Pending couple (used for invite/leave logic).</summary>
    Task<Couple?> GetCurrentCoupleAsync(Guid userId, CancellationToken cancellationToken = default);

    Task<bool> HasActiveCoupleAsync(Guid userId, CancellationToken cancellationToken = default);
}
