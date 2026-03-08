namespace Birdie69.Domain.Common;

/// <summary>
/// Base class for all domain events.
/// Domain events represent something significant that happened in the domain.
/// </summary>
public abstract record DomainEvent
{
    public Guid Id { get; } = Guid.NewGuid();
    public DateTime OccurredAt { get; } = DateTime.UtcNow;
}
