using AutoMapper;
using Birdie69.Application.Common.Mappings;
using Birdie69.Application.Features.Answers.Queries.GetAnswerHistory;

namespace Birdie69.Application.Tests.Features;

public sealed class GetAnswerHistoryQueryHandlerTests
{
    private readonly Mock<IAnswerRepository> _answerRepo = new();
    private readonly Mock<ICoupleRepository> _coupleRepo = new();
    private readonly Mock<IUserRepository> _userRepo = new();
    private readonly Mock<ICurrentUser> _currentUser = new();
    private readonly IMapper _mapper = new MapperConfiguration(c => c.AddProfile<MappingProfile>()).CreateMapper();

    private GetAnswerHistoryQueryHandler CreateHandler() => new(
        _answerRepo.Object, _coupleRepo.Object, _userRepo.Object, _currentUser.Object, _mapper);

    private static User MakeUser(string externalId = "ext-user")
    {
        var user = User.Create(Guid.NewGuid(), externalId, "Test User");
        user.ClearDomainEvents();
        return user;
    }

    private static Couple MakeCouple(Guid userId)
    {
        var couple = Couple.Create(Guid.NewGuid(), userId);
        couple.AcceptInvite(Guid.NewGuid());
        couple.ClearDomainEvents();
        return couple;
    }

    private static Question MakeQuestion(DateOnly scheduledDate)
        => Question.Create(Guid.NewGuid(), $"ext-{Guid.NewGuid()}", "Test Title", "Test Body", "fun", scheduledDate);

    private static Answer MakeAnswer(Guid userId, Guid questionId, Guid coupleId)
    {
        var answer = Answer.Submit(Guid.NewGuid(), userId, questionId, coupleId, "My answer text");
        answer.ClearDomainEvents();
        return answer;
    }

    [Fact]
    public async Task Handle_EmptyHistory_ReturnsEmptyListAndZeroCount()
    {
        var user = MakeUser();
        var couple = MakeCouple(user.Id);

        _currentUser.Setup(x => x.ExternalId).Returns("ext-user");
        _userRepo.Setup(x => x.GetByExternalIdAsync("ext-user", default)).ReturnsAsync(user);
        _coupleRepo.Setup(x => x.GetActiveByUserIdAsync(user.Id, default)).ReturnsAsync(couple);
        _answerRepo.Setup(x => x.GetHistoryByCoupleAsync(couple.Id, user.Id, 1, 20, default))
            .ReturnsAsync((
                (IReadOnlyList<(Answer, Answer?, Question)>)[],
                0));

        var result = await CreateHandler().Handle(new GetAnswerHistoryQuery(1, 20), default);

        result.IsSuccess.Should().BeTrue();
        result.Value.Items.Should().BeEmpty();
        result.Value.TotalCount.Should().Be(0);
        result.Value.Page.Should().Be(1);
        result.Value.PageSize.Should().Be(20);
    }

    [Fact]
    public async Task Handle_BothAnswered_IsRevealedTrue()
    {
        var user = MakeUser();
        var couple = MakeCouple(user.Id);
        var question = MakeQuestion(DateOnly.FromDateTime(DateTime.UtcNow));
        var myAnswer = MakeAnswer(user.Id, question.Id, couple.Id);
        var partnerId = Guid.NewGuid();
        var partnerAnswer = MakeAnswer(partnerId, question.Id, couple.Id);

        _currentUser.Setup(x => x.ExternalId).Returns("ext-user");
        _userRepo.Setup(x => x.GetByExternalIdAsync("ext-user", default)).ReturnsAsync(user);
        _coupleRepo.Setup(x => x.GetActiveByUserIdAsync(user.Id, default)).ReturnsAsync(couple);
        _answerRepo.Setup(x => x.GetHistoryByCoupleAsync(couple.Id, user.Id, 1, 20, default))
            .ReturnsAsync((
                (IReadOnlyList<(Answer, Answer?, Question)>)[(myAnswer, partnerAnswer, question)],
                1));

        var result = await CreateHandler().Handle(new GetAnswerHistoryQuery(1, 20), default);

        result.IsSuccess.Should().BeTrue();
        result.Value.Items.Should().HaveCount(1);
        result.Value.Items[0].IsRevealed.Should().BeTrue();
        result.Value.Items[0].MyAnswer.Should().NotBeNull();
        result.Value.Items[0].PartnerAnswer.Should().NotBeNull();
    }

    [Fact]
    public async Task Handle_OnlySelfAnswered_IsRevealedFalse()
    {
        var user = MakeUser();
        var couple = MakeCouple(user.Id);
        var question = MakeQuestion(DateOnly.FromDateTime(DateTime.UtcNow));
        var myAnswer = MakeAnswer(user.Id, question.Id, couple.Id);

        _currentUser.Setup(x => x.ExternalId).Returns("ext-user");
        _userRepo.Setup(x => x.GetByExternalIdAsync("ext-user", default)).ReturnsAsync(user);
        _coupleRepo.Setup(x => x.GetActiveByUserIdAsync(user.Id, default)).ReturnsAsync(couple);
        _answerRepo.Setup(x => x.GetHistoryByCoupleAsync(couple.Id, user.Id, 1, 20, default))
            .ReturnsAsync((
                (IReadOnlyList<(Answer, Answer?, Question)>)[(myAnswer, null, question)],
                1));

        var result = await CreateHandler().Handle(new GetAnswerHistoryQuery(1, 20), default);

        result.IsSuccess.Should().BeTrue();
        result.Value.Items[0].IsRevealed.Should().BeFalse();
        result.Value.Items[0].MyAnswer.Should().NotBeNull();
        result.Value.Items[0].PartnerAnswer.Should().BeNull();
    }

    [Fact]
    public async Task Handle_PaginationPage2_PassesCorrectOffsetToRepository()
    {
        var user = MakeUser();
        var couple = MakeCouple(user.Id);

        _currentUser.Setup(x => x.ExternalId).Returns("ext-user");
        _userRepo.Setup(x => x.GetByExternalIdAsync("ext-user", default)).ReturnsAsync(user);
        _coupleRepo.Setup(x => x.GetActiveByUserIdAsync(user.Id, default)).ReturnsAsync(couple);
        _answerRepo.Setup(x => x.GetHistoryByCoupleAsync(couple.Id, user.Id, 2, 10, default))
            .ReturnsAsync((
                (IReadOnlyList<(Answer, Answer?, Question)>)[],
                25));

        var result = await CreateHandler().Handle(new GetAnswerHistoryQuery(2, 10), default);

        result.IsSuccess.Should().BeTrue();
        result.Value.Page.Should().Be(2);
        result.Value.PageSize.Should().Be(10);
        result.Value.TotalCount.Should().Be(25);
        _answerRepo.Verify(x => x.GetHistoryByCoupleAsync(couple.Id, user.Id, 2, 10, default), Times.Once);
    }
}
