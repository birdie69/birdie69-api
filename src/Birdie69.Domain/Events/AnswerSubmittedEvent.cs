using Birdie69.Domain.Common;

namespace Birdie69.Domain.Events;

public sealed record AnswerSubmittedEvent(
    Guid AnswerId,
    Guid UserId,
    Guid QuestionId,
    Guid CoupleId) : DomainEvent;
