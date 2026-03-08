using Birdie69.Domain.Entities;

namespace Birdie69.Application.Features.Couples.Queries.GetCouple;

public sealed record CoupleDto(
    Guid Id,
    Guid InitiatorId,
    Guid? PartnerId,
    string InviteCode,
    CoupleStatus Status,
    TimeOnly NotificationTime);
