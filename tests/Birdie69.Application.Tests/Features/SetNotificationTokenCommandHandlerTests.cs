using Birdie69.Application.Features.Users.Commands.SetNotificationToken;

namespace Birdie69.Application.Tests.Features;

public sealed class SetNotificationTokenCommandHandlerTests
{
    private readonly Mock<IUserRepository> _userRepo = new();
    private readonly Mock<ICurrentUser> _currentUser = new();
    private readonly Mock<IUnitOfWork> _unitOfWork = new();

    private SetNotificationTokenCommandHandler CreateHandler() =>
        new(_userRepo.Object, _currentUser.Object, _unitOfWork.Object);

    private User SetupUser(string externalId = "ext-001")
    {
        var user = User.Create(Guid.NewGuid(), externalId, "Alice");
        _currentUser.Setup(c => c.ExternalId).Returns(externalId);
        _userRepo.Setup(r => r.GetByExternalIdAsync(externalId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);
        return user;
    }

    [Fact]
    public async Task Handle_ValidToken_SavesToken()
    {
        var user = SetupUser();
        const string token = "fake-fcm-token-abc123";

        var result = await CreateHandler().Handle(new SetNotificationTokenCommand(token), default);

        result.IsSuccess.Should().BeTrue();
        user.NotificationToken.Should().Be(token);
        _userRepo.Verify(r => r.Update(user), Times.Once);
        _unitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_WhenUserNotFound_ReturnsNotFound()
    {
        _currentUser.Setup(c => c.ExternalId).Returns("unknown");
        _userRepo.Setup(r => r.GetByExternalIdAsync("unknown", It.IsAny<CancellationToken>()))
            .ReturnsAsync((User?)null);

        var result = await CreateHandler().Handle(new SetNotificationTokenCommand("some-token"), default);

        result.IsFailure.Should().BeTrue();
        result.Error.Type.Should().Be(ErrorType.NotFound);
    }
}
