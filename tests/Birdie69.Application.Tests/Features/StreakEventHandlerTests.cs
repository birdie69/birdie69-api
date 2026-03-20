using Birdie69.Application.Common;
using Birdie69.Application.Features.Streaks.Events;
using Birdie69.Domain.Events;

namespace Birdie69.Application.Tests.Features;

public sealed class StreakEventHandlerTests
{
    private readonly Mock<IStreakRepository> _streakRepo = new();
    private readonly Mock<IUnitOfWork> _uow = new();

    private AnswerSubmittedEventHandler CreateHandler() =>
        new(_streakRepo.Object, _uow.Object);

    private static DomainEventNotification<AnswerSubmittedEvent> MakeNotification(Guid userId)
        => new(new AnswerSubmittedEvent(Guid.NewGuid(), userId, Guid.NewGuid(), Guid.NewGuid()));

    [Fact]
    public async Task Handle_NoExistingStreak_CreatesAndRecordsActivity()
    {
        var userId = Guid.NewGuid();
        _streakRepo.Setup(r => r.GetByUserIdAsync(userId, default))
            .ReturnsAsync((Streak?)null);

        Streak? addedStreak = null;
        _streakRepo.Setup(r => r.AddAsync(It.IsAny<Streak>(), default))
            .Callback<Streak, CancellationToken>((s, _) => addedStreak = s);

        await CreateHandler().Handle(MakeNotification(userId), default);

        addedStreak.Should().NotBeNull();
        addedStreak!.CurrentCount.Should().Be(1);
        addedStreak.LongestCount.Should().Be(1);
        _uow.Verify(u => u.SaveChangesAsync(default), Times.Once);
    }

    [Fact]
    public async Task Handle_ExistingStreakDifferentDay_IncrementsCount()
    {
        var userId = Guid.NewGuid();
        var streak = Streak.Create(Guid.NewGuid(), userId);
        var yesterday = DateOnly.FromDateTime(DateTime.UtcNow).AddDays(-1);
        streak.RecordActivity(yesterday);

        _streakRepo.Setup(r => r.GetByUserIdAsync(userId, default)).ReturnsAsync(streak);

        await CreateHandler().Handle(MakeNotification(userId), default);

        streak.CurrentCount.Should().Be(2);
        _uow.Verify(u => u.SaveChangesAsync(default), Times.Once);
    }

    [Fact]
    public async Task Handle_ExistingStreakGap_ResetsToOne()
    {
        var userId = Guid.NewGuid();
        var streak = Streak.Create(Guid.NewGuid(), userId);
        var twoDaysAgo = DateOnly.FromDateTime(DateTime.UtcNow).AddDays(-2);
        streak.RecordActivity(twoDaysAgo);

        _streakRepo.Setup(r => r.GetByUserIdAsync(userId, default)).ReturnsAsync(streak);

        await CreateHandler().Handle(MakeNotification(userId), default);

        streak.CurrentCount.Should().Be(1);
        _uow.Verify(u => u.SaveChangesAsync(default), Times.Once);
    }

    [Fact]
    public async Task Handle_SameDaySubmitTwice_IsIdempotent()
    {
        var userId = Guid.NewGuid();
        var streak = Streak.Create(Guid.NewGuid(), userId);
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        streak.RecordActivity(today);

        _streakRepo.Setup(r => r.GetByUserIdAsync(userId, default)).ReturnsAsync(streak);

        await CreateHandler().Handle(MakeNotification(userId), default);

        streak.CurrentCount.Should().Be(1);
        _uow.Verify(u => u.SaveChangesAsync(default), Times.Never);
    }

    [Fact]
    public async Task Handle_ConsecutiveDays_LongestCountUpdated()
    {
        var userId = Guid.NewGuid();
        var streak = Streak.Create(Guid.NewGuid(), userId);
        var yesterday = DateOnly.FromDateTime(DateTime.UtcNow).AddDays(-1);
        streak.RecordActivity(yesterday);

        _streakRepo.Setup(r => r.GetByUserIdAsync(userId, default)).ReturnsAsync(streak);

        await CreateHandler().Handle(MakeNotification(userId), default);

        streak.CurrentCount.Should().Be(2);
        streak.LongestCount.Should().Be(2);
    }
}
