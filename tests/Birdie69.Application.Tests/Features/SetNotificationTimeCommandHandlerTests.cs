using Birdie69.Application.Features.Couples.Commands.SetNotificationTime;

namespace Birdie69.Application.Tests.Features;

public sealed class SetNotificationTimeCommandHandlerTests
{
    private readonly Mock<ICoupleRepository> _coupleRepo = new();
    private readonly Mock<IUserRepository> _userRepo = new();
    private readonly Mock<ICurrentUser> _currentUser = new();
    private readonly Mock<IUnitOfWork> _unitOfWork = new();

    private SetNotificationTimeCommandHandler CreateHandler() =>
        new(_coupleRepo.Object, _userRepo.Object, _currentUser.Object, _unitOfWork.Object);

    private User SetupUser(string externalId = "ext-001")
    {
        var user = User.Create(Guid.NewGuid(), externalId, "Alice");
        _currentUser.Setup(c => c.ExternalId).Returns(externalId);
        _userRepo.Setup(r => r.GetByExternalIdAsync(externalId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);
        return user;
    }

    [Fact]
    public async Task Handle_ValidTime_SavesNotificationTime()
    {
        var user = SetupUser();
        var couple = Couple.Create(Guid.NewGuid(), user.Id);
        _coupleRepo.Setup(r => r.GetCurrentCoupleAsync(user.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(couple);

        var result = await CreateHandler().Handle(new SetNotificationTimeCommand("20:30"), default);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be("20:30");
        couple.NotificationTime.Should().Be(new TimeOnly(20, 30));
        _coupleRepo.Verify(r => r.Update(couple), Times.Once);
        _unitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_WhenNotInCouple_ReturnsNotFound()
    {
        var user = SetupUser();
        _coupleRepo.Setup(r => r.GetCurrentCoupleAsync(user.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Couple?)null);

        var result = await CreateHandler().Handle(new SetNotificationTimeCommand("08:00"), default);

        result.IsFailure.Should().BeTrue();
        result.Error.Type.Should().Be(ErrorType.NotFound);
    }

    [Fact]
    public async Task Handle_WhenUserNotFound_ReturnsNotFound()
    {
        _currentUser.Setup(c => c.ExternalId).Returns("unknown");
        _userRepo.Setup(r => r.GetByExternalIdAsync("unknown", It.IsAny<CancellationToken>()))
            .ReturnsAsync((User?)null);

        var result = await CreateHandler().Handle(new SetNotificationTimeCommand("08:00"), default);

        result.IsFailure.Should().BeTrue();
        result.Error.Type.Should().Be(ErrorType.NotFound);
    }
}
