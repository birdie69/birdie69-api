using Birdie69.Domain.Entities;
using Birdie69.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Birdie69.Infrastructure.Persistence.Repositories;

public sealed class UserRepository(AppDbContext context)
    : Repository<User>(context), IUserRepository
{
    public async Task<User?> GetByExternalIdAsync(string externalId, CancellationToken cancellationToken = default)
        => await DbSet.FirstOrDefaultAsync(u => u.ExternalId == externalId, cancellationToken);

    public async Task<bool> ExistsByExternalIdAsync(string externalId, CancellationToken cancellationToken = default)
        => await DbSet.AnyAsync(u => u.ExternalId == externalId, cancellationToken);
}
