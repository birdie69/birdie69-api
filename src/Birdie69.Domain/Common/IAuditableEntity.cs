namespace Birdie69.Domain.Common;

/// <summary>
/// Marks an entity as auditable — EF Core interceptor will populate these fields automatically.
/// </summary>
public interface IAuditableEntity
{
    DateTime CreatedAt { get; }
    DateTime UpdatedAt { get; }
    string? CreatedBy { get; }
    string? UpdatedBy { get; }
}
