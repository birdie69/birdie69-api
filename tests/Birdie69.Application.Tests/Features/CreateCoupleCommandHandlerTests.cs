using Birdie69.Application.Features.Couples.Commands.CreateCouple;

namespace Birdie69.Application.Tests.Features;

public sealed class CreateCoupleCommandHandlerTests
{
    private readonly Mock<ICoupleRepository> _coupleRepo = new();
    private readonly Mock<IUserRepository> _userRepo = new();
    private readonly Mock<ICurrentUser> _currentUser = new();
    private readonly Mock<IUnitOfWork> _unitOfWork = new();

    private CreateCoupleCommandHandler CreateHandler() =>
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
    public async Task Handle_WhenNoExistingCouple_CreatesNewCouple()
    {
        var user = SetupUser();
        _coupleRepo.Setup(r => r.GetCurrentCoupleAsync(user.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Couple?)null);

        var result = await CreateHandler().Handle(new CreateCoupleCommand(), default);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().HaveLength(8);
        _coupleRepo.Verify(r => r.AddAsync(It.IsAny<Couple>(), It.IsAny<CancellationToken>()), Times.Once);
        _unitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_WhenPendingCoupleExists_RegeneratesCodeAndReturnsIt()
    {
        var user = SetupUser();
        var pendingCouple = Couple.Create(Guid.NewGuid(), user.Id);
        var originalCode = pendingCouple.InviteCode.Value;

        _coupleRepo.Setup(r => r.GetCurrentCoupleAsync(user.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(pendingCouple);

        var result = await CreateHandler().Handle(new CreateCoupleCommand(), default);

        result.IsSuccess.Should().BeTrue();
        // Code might be the same (random) but the operation should succeed
        result.Value.Should().HaveLength(8);
        _coupleRepo.Verify(r => r.Update(pendingCouple), Times.Once);
        _coupleRepo.Verify(r => r.AddAsync(It.IsAny<Couple>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_WhenActiveCoupleExists_ReturnsConflict()
    {
        var user = SetupUser();
        var activeCouple = Couple.Create(Guid.NewGuid(), user.Id);
        activeCouple.AcceptInvite(Guid.NewGuid());

        _coupleRepo.Setup(r => r.GetCurrentCoupleAsync(user.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(activeCouple);

        var result = await CreateHandler().Handle(new CreateCoupleCommand(), default);

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Couple.AlreadyExists");
        result.Error.Type.Should().Be(ErrorType.Conflict);
    }
}
