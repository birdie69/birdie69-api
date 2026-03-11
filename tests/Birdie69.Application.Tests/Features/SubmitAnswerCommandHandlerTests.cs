using Birdie69.Application.Features.Answers.Commands.SubmitAnswer;

namespace Birdie69.Application.Tests.Features;

public sealed class SubmitAnswerCommandHandlerTests
{
    private readonly Mock<IAnswerRepository> _answerRepo = new();
    private readonly Mock<IQuestionRepository> _questionRepo = new();
    private readonly Mock<ICoupleRepository> _coupleRepo = new();
    private readonly Mock<IUserRepository> _userRepo = new();
    private readonly Mock<ICurrentUser> _currentUser = new();
    private readonly Mock<IUnitOfWork> _uow = new();

    private SubmitAnswerCommandHandler CreateHandler() => new(
        _answerRepo.Object, _questionRepo.Object, _coupleRepo.Object,
        _userRepo.Object, _currentUser.Object, _uow.Object);

    [Fact]
    public async Task Handle_WhenUserNotFound_ReturnsFailure()
    {
        _currentUser.Setup(x => x.ExternalId).Returns("ext123");
        _userRepo.Setup(x => x.GetByExternalIdAsync("ext123", default))
            .ReturnsAsync((User?)null);

        var result = await CreateHandler().Handle(
            new SubmitAnswerCommand(Guid.NewGuid(), "My answer"), default);

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Contain("NotFound");
    }

    [Fact]
    public async Task Handle_WhenAlreadyAnswered_ReturnsConflict()
    {
        var userId = Guid.NewGuid();
        var questionId = Guid.NewGuid();
        var user = User.Create(userId, "ext123", "Alice");
        user.ClearDomainEvents();
        var couple = Couple.Create(Guid.NewGuid(), userId);
        couple.AcceptInvite(Guid.NewGuid());
        couple.ClearDomainEvents();
        var question = Question.Create(questionId, "ext-q1", "Question?", DateOnly.FromDateTime(DateTime.UtcNow));
        var existingAnswer = Answer.Submit(Guid.NewGuid(), userId, questionId, couple.Id, "Previous");
        existingAnswer.ClearDomainEvents();

        _currentUser.Setup(x => x.ExternalId).Returns("ext123");
        _userRepo.Setup(x => x.GetByExternalIdAsync("ext123", default)).ReturnsAsync(user);
        _coupleRepo.Setup(x => x.GetActiveByUserIdAsync(userId, default)).ReturnsAsync(couple);
        _questionRepo.Setup(x => x.GetByIdAsync(questionId, default)).ReturnsAsync(question);
        _answerRepo.Setup(x => x.GetByUserAndQuestionAsync(userId, questionId, default)).ReturnsAsync(existingAnswer);

        var result = await CreateHandler().Handle(
            new SubmitAnswerCommand(questionId, "New answer"), default);

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Answer.AlreadySubmitted");
    }
}
