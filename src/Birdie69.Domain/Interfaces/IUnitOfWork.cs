namespace Birdie69.Domain.Interfaces;

/// <summary>
/// Unit of Work pattern — wraps a database transaction.
/// Ensures all repository changes within a use case are committed atomically.
/// </summary>
public interface IUnitOfWork
{
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
