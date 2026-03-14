using Birdie69.Application.Features.Couples.Commands.LeaveCouple;

namespace Birdie69.Application.Tests.Features;

public sealed class LeaveCoupleCommandHandlerTests
{
    private readonly Mock<ICoupleRepository> _coupleRepo = new();
    private readonly Mock<IUserRepository> _userRepo = new();
    private readonly Mock<ICurrentUser> _currentUser = new();
    private readonly Mock<IUnitOfWork> _unitOfWork = new();

    private LeaveCoupleCommandHandler CreateHandler() =>
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
    public async Task Handle_WhenPendingAndCallerIsInitiator_CancelsCouple()
    {
        var initiator = SetupUser();
        var couple = Couple.Create(Guid.NewGuid(), initiator.Id);
        _coupleRepo.Setup(r => r.GetCurrentCoupleAsync(initiator.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(couple);

        var result = await CreateHandler().Handle(new LeaveCoupleCommand(), default);

        result.IsSuccess.Should().BeTrue();
        couple.Status.Should().Be(CoupleStatus.Cancelled);
        _coupleRepo.Verify(r => r.Update(couple), Times.Once);
    }

    [Fact]
    public async Task Handle_WhenActiveAndCallerIsMember_DisbandsCouple()
    {
        var member = SetupUser();
        var other = User.Create(Guid.NewGuid(), "ext-002", "Bob");
        var couple = Couple.Create(Guid.NewGuid(), other.Id);
        couple.AcceptInvite(member.Id);  // member is partner

        _coupleRepo.Setup(r => r.GetCurrentCoupleAsync(member.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(couple);

        var result = await CreateHandler().Handle(new LeaveCoupleCommand(), default);

        result.IsSuccess.Should().BeTrue();
        couple.Status.Should().Be(CoupleStatus.Disbanded);
    }

    [Fact]
    public async Task Handle_WhenNotInCouple_ReturnsNotFound()
    {
        var user = SetupUser();
        _coupleRepo.Setup(r => r.GetCurrentCoupleAsync(user.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Couple?)null);

        var result = await CreateHandler().Handle(new LeaveCoupleCommand(), default);

        result.IsFailure.Should().BeTrue();
        result.Error.Type.Should().Be(ErrorType.NotFound);
    }

    [Fact]
    public async Task Handle_WhenPendingAndCallerIsNotInitiator_ReturnsConflict()
    {
        var nonInitiator = SetupUser("ext-999");
        var initiator = User.Create(Guid.NewGuid(), "ext-001", "Alice");
        var couple = Couple.Create(Guid.NewGuid(), initiator.Id);

        // Manually fake that nonInitiator is returned for their own lookup but couple belongs to someone else
        // In practice this state shouldn't occur (pending couple has no partner), but we test the guard
        _coupleRepo.Setup(r => r.GetCurrentCoupleAsync(nonInitiator.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(couple);  // couple's initiator is NOT nonInitiator

        var result = await CreateHandler().Handle(new LeaveCoupleCommand(), default);

        result.IsFailure.Should().BeTrue();
        result.Error.Type.Should().Be(ErrorType.Unauthorized);
    }
}
