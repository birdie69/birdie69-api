using Birdie69.Domain.Common;

namespace Birdie69.Domain.Entities;

/// <summary>
/// Tracks a user's daily engagement streak.
/// A streak increments when the user submits an answer on consecutive days.
/// </summary>
public sealed class Streak : AuditableEntity
{
    public Guid UserId { get; private set; }
    public int CurrentCount { get; private set; }
    public int LongestCount { get; private set; }
    public DateOnly LastActivityDate { get; private set; }

    private Streak() { }

    public static Streak Create(Guid id, Guid userId)
    {
        return new Streak
        {
            Id = id,
            UserId = userId,
            CurrentCount = 0,
            LongestCount = 0,
        };
    }

    public void RecordActivity(DateOnly today)
    {
        var daysSinceLast = LastActivityDate == default
            ? 1
            : today.DayNumber - LastActivityDate.DayNumber;

        CurrentCount = daysSinceLast == 1 ? CurrentCount + 1 : 1;

        if (CurrentCount > LongestCount)
            LongestCount = CurrentCount;

        LastActivityDate = today;
    }

    public bool IsActiveToday(DateOnly today) =>
        LastActivityDate == today;
}
