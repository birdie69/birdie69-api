using AutoMapper;
using Birdie69.Application.Common.Mappings;
using Birdie69.Application.Features.Answers.Queries.GetAnswers;

namespace Birdie69.Application.Tests.Features;

public sealed class GetAnswersQueryHandlerTests
{
    private readonly Mock<IAnswerRepository> _answerRepo = new();
    private readonly Mock<ICoupleRepository> _coupleRepo = new();
    private readonly Mock<IUserRepository> _userRepo = new();
    private readonly Mock<ICurrentUser> _currentUser = new();
    private readonly IMapper _mapper = new MapperConfiguration(
        cfg => cfg.AddProfile<MappingProfile>()).CreateMapper();

    private GetAnswersQueryHandler CreateHandler() => new(
        _answerRepo.Object, _coupleRepo.Object, _userRepo.Object,
        _currentUser.Object, _mapper);

    private static User MakeUser(Guid? id = null)
    {
        var u = User.Create(id ?? Guid.NewGuid(), "ext-1", "Alice");
        u.ClearDomainEvents();
        return u;
    }

    private static Couple MakeCouple(Guid initiatorId, Guid partnerId)
    {
        var c = Couple.Create(Guid.NewGuid(), initiatorId);
        c.AcceptInvite(partnerId);
        c.ClearDomainEvents();
        return c;
    }

    private static Answer MakeAnswer(Guid userId, Guid questionId, Guid coupleId, string text = "My answer")
    {
        var a = Answer.Submit(Guid.NewGuid(), userId, questionId, coupleId, text);
        a.ClearDomainEvents();
        return a;
    }

    [Fact]
    public async Task Handle_NeitherAnswered_IsRevealedFalse_BothNull()
    {
        var user = MakeUser();
        var partner = MakeUser();
        var couple = MakeCouple(user.Id, partner.Id);
        var questionId = Guid.NewGuid();

        _currentUser.Setup(x => x.ExternalId).Returns("ext-1");
        _userRepo.Setup(x => x.GetByExternalIdAsync("ext-1", default)).ReturnsAsync(user);
        _coupleRepo.Setup(x => x.GetActiveByUserIdAsync(user.Id, default)).ReturnsAsync(couple);
        _answerRepo.Setup(x => x.GetByUserAndQuestionAsync(user.Id, questionId, default)).ReturnsAsync((Answer?)null);
        _answerRepo.Setup(x => x.BothPartnersAnsweredAsync(questionId, couple.Id, default)).ReturnsAsync(false);

        var result = await CreateHandler().Handle(new GetAnswersQuery(questionId), default);

        result.IsSuccess.Should().BeTrue();
        result.Value.IsRevealed.Should().BeFalse();
        result.Value.MyAnswer.Should().BeNull();
        result.Value.PartnerAnswer.Should().BeNull();
    }

    [Fact]
    public async Task Handle_OnlyCallerAnswered_IsRevealedFalse_MyAnswerFilled_PartnerNull()
    {
        var user = MakeUser();
        var partner = MakeUser();
        var couple = MakeCouple(user.Id, partner.Id);
        var questionId = Guid.NewGuid();
        var myAnswer = MakeAnswer(user.Id, questionId, couple.Id, "My answer");

        _currentUser.Setup(x => x.ExternalId).Returns("ext-1");
        _userRepo.Setup(x => x.GetByExternalIdAsync("ext-1", default)).ReturnsAsync(user);
        _coupleRepo.Setup(x => x.GetActiveByUserIdAsync(user.Id, default)).ReturnsAsync(couple);
        _answerRepo.Setup(x => x.GetByUserAndQuestionAsync(user.Id, questionId, default)).ReturnsAsync(myAnswer);
        _answerRepo.Setup(x => x.BothPartnersAnsweredAsync(questionId, couple.Id, default)).ReturnsAsync(false);

        var result = await CreateHandler().Handle(new GetAnswersQuery(questionId), default);

        result.IsSuccess.Should().BeTrue();
        result.Value.IsRevealed.Should().BeFalse();
        result.Value.MyAnswer.Should().NotBeNull();
        result.Value.MyAnswer!.Text.Should().Be("My answer");
        result.Value.PartnerAnswer.Should().BeNull();
    }

    [Fact]
    public async Task Handle_BothAnswered_IsRevealedTrue_BothFilled()
    {
        var user = MakeUser();
        var partner = MakeUser();
        var couple = MakeCouple(user.Id, partner.Id);
        var questionId = Guid.NewGuid();
        var myAnswer = MakeAnswer(user.Id, questionId, couple.Id, "My answer");
        var partnerAnswer = MakeAnswer(partner.Id, questionId, couple.Id, "Partner answer");

        _currentUser.Setup(x => x.ExternalId).Returns("ext-1");
        _userRepo.Setup(x => x.GetByExternalIdAsync("ext-1", default)).ReturnsAsync(user);
        _coupleRepo.Setup(x => x.GetActiveByUserIdAsync(user.Id, default)).ReturnsAsync(couple);
        _answerRepo.Setup(x => x.GetByUserAndQuestionAsync(user.Id, questionId, default)).ReturnsAsync(myAnswer);
        _answerRepo.Setup(x => x.BothPartnersAnsweredAsync(questionId, couple.Id, default)).ReturnsAsync(true);
        _answerRepo.Setup(x => x.GetByQuestionAndCoupleAsync(questionId, couple.Id, default))
            .ReturnsAsync(new List<Answer> { myAnswer, partnerAnswer });

        var result = await CreateHandler().Handle(new GetAnswersQuery(questionId), default);

        result.IsSuccess.Should().BeTrue();
        result.Value.IsRevealed.Should().BeTrue();
        result.Value.MyAnswer.Should().NotBeNull();
        result.Value.MyAnswer!.Text.Should().Be("My answer");
        result.Value.PartnerAnswer.Should().NotBeNull();
        result.Value.PartnerAnswer!.Text.Should().Be("Partner answer");
    }

    [Fact]
    public async Task Handle_UserNotFound_ReturnsNotFound()
    {
        _currentUser.Setup(x => x.ExternalId).Returns("unknown");
        _userRepo.Setup(x => x.GetByExternalIdAsync("unknown", default)).ReturnsAsync((User?)null);

        var result = await CreateHandler().Handle(new GetAnswersQuery(Guid.NewGuid()), default);

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Contain("NotFound");
    }

    [Fact]
    public async Task Handle_NoActiveCouple_ReturnsConflict()
    {
        var user = MakeUser();
        _currentUser.Setup(x => x.ExternalId).Returns("ext-1");
        _userRepo.Setup(x => x.GetByExternalIdAsync("ext-1", default)).ReturnsAsync(user);
        _coupleRepo.Setup(x => x.GetActiveByUserIdAsync(user.Id, default)).ReturnsAsync((Couple?)null);

        var result = await CreateHandler().Handle(new GetAnswersQuery(Guid.NewGuid()), default);

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Answer.NoCouple");
    }
}
