using Birdie69.Domain.Entities;
using Birdie69.Domain.Events;
using FluentAssertions;

namespace Birdie69.Domain.Tests.Entities;

public sealed class CoupleTests
{
    [Fact]
    public void Create_ShouldRaiseCoupleInviteCreatedEvent()
    {
        var couple = Couple.Create(Guid.NewGuid(), Guid.NewGuid());

        couple.DomainEvents.Should().ContainSingle()
            .Which.Should().BeOfType<CoupleInviteCreatedEvent>();
    }

    [Fact]
    public void AcceptInvite_WithValidPartner_ShouldActivateCouple()
    {
        var couple = Couple.Create(Guid.NewGuid(), Guid.NewGuid());
        couple.ClearDomainEvents();

        var result = couple.AcceptInvite(Guid.NewGuid());

        result.IsSuccess.Should().BeTrue();
        couple.Status.Should().Be(CoupleStatus.Active);
        couple.DomainEvents.Should().ContainSingle()
            .Which.Should().BeOfType<CoupleFormedEvent>();
    }

    [Fact]
    public void AcceptInvite_WithSameUser_ShouldFail()
    {
        var userId = Guid.NewGuid();
        var couple = Couple.Create(Guid.NewGuid(), userId);

        var result = couple.AcceptInvite(userId);

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Couple.SameUser");
    }

    [Fact]
    public void AcceptInvite_WhenAlreadyActive_ShouldFail()
    {
        var couple = Couple.Create(Guid.NewGuid(), Guid.NewGuid());
        couple.AcceptInvite(Guid.NewGuid());

        var result = couple.AcceptInvite(Guid.NewGuid());

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Couple.AlreadyActive");
    }
}
