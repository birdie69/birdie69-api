using Birdie69.Application.Common.Interfaces;
using Birdie69.Domain.Common;
using Birdie69.Domain.Entities;
using Birdie69.Domain.Interfaces;
using MediatR;

namespace Birdie69.Application.Features.Answers.Commands.SubmitAnswer;

public sealed class SubmitAnswerCommandHandler(
    IAnswerRepository answerRepository,
    IQuestionRepository questionRepository,
    ICoupleRepository coupleRepository,
    IUserRepository userRepository,
    ICurrentUser currentUser,
    IUnitOfWork unitOfWork)
    : IRequestHandler<SubmitAnswerCommand, Result<Guid>>
{
    public async Task<Result<Guid>> Handle(
        SubmitAnswerCommand request,
        CancellationToken cancellationToken)
    {
        var user = await userRepository.GetByExternalIdAsync(currentUser.ExternalId, cancellationToken);
        if (user is null)
            return Result.Failure<Guid>(Error.NotFound("User", currentUser.ExternalId));

        var couple = await coupleRepository.GetActiveByUserIdAsync(user.Id, cancellationToken);
        if (couple is null)
            return Result.Failure<Guid>(Error.Conflict("Answer.NoCouple", "You must be in an active couple to answer."));

        var question = await questionRepository.GetByIdAsync(request.QuestionId, cancellationToken);
        if (question is null)
            return Result.Failure<Guid>(Error.NotFound("Question", request.QuestionId));

        var existing = await answerRepository.GetByUserAndQuestionAsync(user.Id, question.Id, cancellationToken);
        if (existing is not null)
            return Result.Failure<Guid>(Error.Conflict("Answer.AlreadySubmitted", "You have already answered this question."));

        var answer = Answer.Submit(Guid.NewGuid(), user.Id, question.Id, couple.Id, request.Text);
        await answerRepository.AddAsync(answer, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return answer.Id;
    }
}
