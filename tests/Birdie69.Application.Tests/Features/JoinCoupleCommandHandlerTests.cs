using Birdie69.Application.Features.Couples.Commands.JoinCouple;

namespace Birdie69.Application.Tests.Features;

public sealed class JoinCoupleCommandHandlerTests
{
    private readonly Mock<ICoupleRepository> _coupleRepo = new();
    private readonly Mock<IUserRepository> _userRepo = new();
    private readonly Mock<ICurrentUser> _currentUser = new();
    private readonly Mock<IUnitOfWork> _unitOfWork = new();

    private JoinCoupleCommandHandler CreateHandler() =>
        new(_coupleRepo.Object, _userRepo.Object, _currentUser.Object, _unitOfWork.Object);

    private User SetupUser(string externalId = "ext-002")
    {
        var user = User.Create(Guid.NewGuid(), externalId, "Bob");
        _currentUser.Setup(c => c.ExternalId).Returns(externalId);
        _userRepo.Setup(r => r.GetByExternalIdAsync(externalId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);
        return user;
    }

    [Fact]
    public async Task Handle_WithValidCode_JoinsCouple()
    {
        var joiner = SetupUser();
        var initiator = User.Create(Guid.NewGuid(), "ext-001", "Alice");
        var couple = Couple.Create(Guid.NewGuid(), initiator.Id);
        var code = couple.InviteCode.Value;

        _coupleRepo.Setup(r => r.HasActiveCoupleAsync(joiner.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);
        _coupleRepo.Setup(r => r.GetByInviteCodeAsync(code, It.IsAny<CancellationToken>()))
            .ReturnsAsync(couple);

        var result = await CreateHandler().Handle(new JoinCoupleCommand(code), default);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be(couple.Id);
        couple.Status.Should().Be(CoupleStatus.Active);
        _coupleRepo.Verify(r => r.Update(couple), Times.Once);
    }

    [Fact]
    public async Task Handle_WhenCodeNotFound_ReturnsNotFound()
    {
        var joiner = SetupUser();
        _coupleRepo.Setup(r => r.HasActiveCoupleAsync(joiner.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);
        _coupleRepo.Setup(r => r.GetByInviteCodeAsync("XXXXXXXX", It.IsAny<CancellationToken>()))
            .ReturnsAsync((Couple?)null);

        var result = await CreateHandler().Handle(new JoinCoupleCommand("XXXXXXXX"), default);

        result.IsFailure.Should().BeTrue();
        result.Error.Type.Should().Be(ErrorType.NotFound);
    }

    [Fact]
    public async Task Handle_WhenJoinerAlreadyInActiveCouple_ReturnsConflict()
    {
        var joiner = SetupUser();
        _coupleRepo.Setup(r => r.HasActiveCoupleAsync(joiner.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var result = await CreateHandler().Handle(new JoinCoupleCommand("ABCD1234"), default);

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Couple.AlreadyExists");
        result.Error.Type.Should().Be(ErrorType.Conflict);
    }

    [Fact]
    public async Task Handle_WhenJoiningOwnCouple_ReturnsSameUserValidationError()
    {
        var joiner = SetupUser();
        var couple = Couple.Create(Guid.NewGuid(), joiner.Id);  // joiner is initiator
        var code = couple.InviteCode.Value;

        _coupleRepo.Setup(r => r.HasActiveCoupleAsync(joiner.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);
        _coupleRepo.Setup(r => r.GetByInviteCodeAsync(code, It.IsAny<CancellationToken>()))
            .ReturnsAsync(couple);

        var result = await CreateHandler().Handle(new JoinCoupleCommand(code), default);

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Couple.SameUser");
    }
}
