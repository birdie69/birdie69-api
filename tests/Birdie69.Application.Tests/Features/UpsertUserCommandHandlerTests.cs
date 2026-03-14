using Birdie69.Application.Features.Users.Commands.UpsertUser;

namespace Birdie69.Application.Tests.Features;

public sealed class UpsertUserCommandHandlerTests
{
    private readonly Mock<IUserRepository> _userRepo = new();
    private readonly Mock<IUnitOfWork> _unitOfWork = new();

    private UpsertUserCommandHandler CreateHandler() =>
        new(_userRepo.Object, _unitOfWork.Object);

    [Fact]
    public async Task Handle_WhenUserDoesNotExist_CreatesNewUser()
    {
        _userRepo.Setup(r => r.GetByExternalIdAsync("ext-001", It.IsAny<CancellationToken>()))
            .ReturnsAsync((User?)null);

        var result = await CreateHandler().Handle(
            new UpsertUserCommand("ext-001", "Alice", null), default);

        result.IsSuccess.Should().BeTrue();
        _userRepo.Verify(r => r.AddAsync(It.Is<User>(u =>
            u.ExternalId == "ext-001" && u.DisplayName == "Alice"), It.IsAny<CancellationToken>()), Times.Once);
        _unitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_WhenUserExists_UpdatesProfile()
    {
        var existing = User.Create(Guid.NewGuid(), "ext-001", "OldName");
        _userRepo.Setup(r => r.GetByExternalIdAsync("ext-001", It.IsAny<CancellationToken>()))
            .ReturnsAsync(existing);

        var result = await CreateHandler().Handle(
            new UpsertUserCommand("ext-001", "NewName", "https://avatar.url/img.png"), default);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be(existing.Id);
        existing.DisplayName.Should().Be("NewName");
        existing.AvatarUrl.Should().Be("https://avatar.url/img.png");
        _userRepo.Verify(r => r.Update(existing), Times.Once);
        _unitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_WhenUserExists_DoesNotCallAddAsync()
    {
        var existing = User.Create(Guid.NewGuid(), "ext-001", "Alice");
        _userRepo.Setup(r => r.GetByExternalIdAsync("ext-001", It.IsAny<CancellationToken>()))
            .ReturnsAsync(existing);

        await CreateHandler().Handle(new UpsertUserCommand("ext-001", "Alice", null), default);

        _userRepo.Verify(r => r.AddAsync(It.IsAny<User>(), It.IsAny<CancellationToken>()), Times.Never);
    }
}
