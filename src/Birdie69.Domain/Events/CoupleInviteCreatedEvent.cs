using Birdie69.Domain.Common;

namespace Birdie69.Domain.Events;

public sealed record CoupleInviteCreatedEvent(
    Guid CoupleId,
    Guid InitiatorId,
    string InviteCode) : DomainEvent;
