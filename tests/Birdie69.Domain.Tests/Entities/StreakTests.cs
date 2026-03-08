using Birdie69.Domain.Entities;
using FluentAssertions;

namespace Birdie69.Domain.Tests.Entities;

public sealed class StreakTests
{
    [Fact]
    public void RecordActivity_OnConsecutiveDays_ShouldIncrementStreak()
    {
        var streak = Streak.Create(Guid.NewGuid(), Guid.NewGuid());
        var day1 = new DateOnly(2026, 3, 1);
        var day2 = new DateOnly(2026, 3, 2);

        streak.RecordActivity(day1);
        streak.RecordActivity(day2);

        streak.CurrentCount.Should().Be(2);
        streak.LongestCount.Should().Be(2);
    }

    [Fact]
    public void RecordActivity_WithGap_ShouldResetStreak()
    {
        var streak = Streak.Create(Guid.NewGuid(), Guid.NewGuid());
        streak.RecordActivity(new DateOnly(2026, 3, 1));
        streak.RecordActivity(new DateOnly(2026, 3, 2));

        streak.RecordActivity(new DateOnly(2026, 3, 5));

        streak.CurrentCount.Should().Be(1);
        streak.LongestCount.Should().Be(2);
    }
}
