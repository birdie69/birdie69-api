namespace Birdie69.Domain.Common;

/// <summary>
/// Base class for auditable domain entities.
/// Combines identity, domain events, and audit trail.
/// </summary>
public abstract class AuditableEntity : Entity, IAuditableEntity
{
    public DateTime CreatedAt { get; private set; }
    public DateTime UpdatedAt { get; private set; }
    public string? CreatedBy { get; private set; }
    public string? UpdatedBy { get; private set; }

    protected AuditableEntity() { }

    protected AuditableEntity(Guid id) : base(id) { }
}
