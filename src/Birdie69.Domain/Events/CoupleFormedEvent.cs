using Birdie69.Domain.Common;

namespace Birdie69.Domain.Events;

public sealed record CoupleFormedEvent(
    Guid CoupleId,
    Guid InitiatorId,
    Guid PartnerId) : DomainEvent;
