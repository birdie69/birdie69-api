using Birdie69.Domain.Entities;

namespace Birdie69.Domain.Interfaces;

public interface IUserRepository : IRepository<User>
{
    Task<User?> GetByExternalIdAsync(string externalId, CancellationToken cancellationToken = default);
    Task<bool> ExistsByExternalIdAsync(string externalId, CancellationToken cancellationToken = default);
}
