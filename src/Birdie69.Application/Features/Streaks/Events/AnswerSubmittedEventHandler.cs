using Birdie69.Application.Common;
using Birdie69.Domain.Entities;
using Birdie69.Domain.Events;
using Birdie69.Domain.Interfaces;
using MediatR;

namespace Birdie69.Application.Features.Streaks.Events;

/// <summary>
/// Handles AnswerSubmittedEvent to update the user's daily streak.
/// Idempotent: if the streak is already recorded today, it is a no-op.
/// </summary>
public sealed class AnswerSubmittedEventHandler(
    IStreakRepository streakRepository,
    IUnitOfWork unitOfWork)
    : INotificationHandler<DomainEventNotification<AnswerSubmittedEvent>>
{
    public async Task Handle(
        DomainEventNotification<AnswerSubmittedEvent> notification,
        CancellationToken cancellationToken)
    {
        var userId = notification.DomainEvent.UserId;
        var today = DateOnly.FromDateTime(DateTime.UtcNow);

        var streak = await streakRepository.GetByUserIdAsync(userId, cancellationToken);

        if (streak is null)
        {
            streak = Streak.Create(Guid.NewGuid(), userId);
            await streakRepository.AddAsync(streak, cancellationToken);
        }

        if (streak.IsActiveToday(today))
            return;

        streak.RecordActivity(today);
        await unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
