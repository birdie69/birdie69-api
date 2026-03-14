using Birdie69.Domain.Common;
using Birdie69.Domain.Events;
using Birdie69.Domain.ValueObjects;

namespace Birdie69.Domain.Entities;

public enum CoupleStatus { Pending, Active, Disbanded, Cancelled }

/// <summary>
/// Represents a relationship between two users.
/// A user can only be in one active couple at a time (enforced via domain rules).
/// </summary>
public sealed class Couple : AuditableEntity
{
    public Guid InitiatorId { get; private set; }
    public Guid? PartnerId { get; private set; }
    public InviteCode InviteCode { get; private set; } = default!;
    public CoupleStatus Status { get; private set; } = CoupleStatus.Pending;
    public TimeOnly NotificationTime { get; private set; } = new TimeOnly(8, 0);

    private Couple() { }

    public static Couple Create(Guid id, Guid initiatorId)
    {
        var couple = new Couple
        {
            Id = id,
            InitiatorId = initiatorId,
            InviteCode = InviteCode.Generate(),
            Status = CoupleStatus.Pending,
        };

        couple.RaiseDomainEvent(new CoupleInviteCreatedEvent(couple.Id, initiatorId, couple.InviteCode.Value));
        return couple;
    }

    public Result AcceptInvite(Guid partnerId)
    {
        if (Status != CoupleStatus.Pending)
            return Result.Failure(Error.Conflict("Couple.AlreadyActive", "Invite is no longer valid."));

        if (InitiatorId == partnerId)
            return Result.Failure(Error.Validation("Couple.SameUser", "Cannot form a couple with yourself."));

        PartnerId = partnerId;
        Status = CoupleStatus.Active;

        RaiseDomainEvent(new CoupleFormedEvent(Id, InitiatorId, partnerId));
        return Result.Success();
    }

    public Result Disband()
    {
        if (Status != CoupleStatus.Active)
            return Result.Failure(Error.Conflict("Couple.NotActive", "Couple is not active."));

        Status = CoupleStatus.Disbanded;
        return Result.Success();
    }

    /// <summary>
    /// Generates a fresh invite code. Only valid while the couple is still Pending.
    /// Replaces the old code so the previous one can no longer be used to join.
    /// </summary>
    public Result RegenerateCode()
    {
        if (Status != CoupleStatus.Pending)
            return Result.Failure(Error.Conflict("Couple.NotPending", "Cannot regenerate code for a non-pending couple."));

        InviteCode = InviteCode.Generate();
        return Result.Success();
    }

    /// <summary>
    /// Cancels a pending invite. Only the initiator may cancel before a partner joins.
    /// </summary>
    public Result Cancel(Guid callerId)
    {
        if (Status != CoupleStatus.Pending)
            return Result.Failure(Error.Conflict("Couple.NotPending", "Only pending couples can be cancelled."));

        if (InitiatorId != callerId)
            return Result.Failure(Error.Unauthorized("Couple.NotInitiator", "Only the initiator can cancel a pending couple."));

        Status = CoupleStatus.Cancelled;
        return Result.Success();
    }

    public void SetNotificationTime(TimeOnly time)
    {
        NotificationTime = time;
    }

    public bool IsMember(Guid userId) =>
        InitiatorId == userId || PartnerId == userId;
}
