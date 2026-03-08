using Birdie69.Domain.Common;

namespace Birdie69.Domain.Events;

public sealed record UserCreatedEvent(
    Guid UserId,
    string ExternalId,
    string DisplayName) : DomainEvent;
