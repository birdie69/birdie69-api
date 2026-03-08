using AutoMapper;
using Birdie69.Application.Common.Interfaces;
using Birdie69.Domain.Common;
using Birdie69.Domain.Interfaces;
using MediatR;

namespace Birdie69.Application.Features.Answers.Queries.GetAnswers;

public sealed class GetAnswersQueryHandler(
    IAnswerRepository answerRepository,
    ICoupleRepository coupleRepository,
    IUserRepository userRepository,
    ICurrentUser currentUser,
    IMapper mapper)
    : IRequestHandler<GetAnswersQuery, Result<IReadOnlyList<AnswerDto>>>
{
    public async Task<Result<IReadOnlyList<AnswerDto>>> Handle(
        GetAnswersQuery request,
        CancellationToken cancellationToken)
    {
        var user = await userRepository.GetByExternalIdAsync(currentUser.ExternalId, cancellationToken);
        if (user is null)
            return Result.Failure<IReadOnlyList<AnswerDto>>(Error.NotFound("User", currentUser.ExternalId));

        var couple = await coupleRepository.GetActiveByUserIdAsync(user.Id, cancellationToken);
        if (couple is null)
            return Result.Failure<IReadOnlyList<AnswerDto>>(Error.Conflict("Answer.NoCouple", "You must be in an active couple."));

        var bothAnswered = await answerRepository.BothPartnersAnsweredAsync(request.QuestionId, couple.Id, cancellationToken);
        if (!bothAnswered)
            return Result.Failure<IReadOnlyList<AnswerDto>>(
                Error.Conflict("Answer.NotBothAnswered", "Answers are only visible after both partners have submitted."));

        var answers = await answerRepository.GetByQuestionAndCoupleAsync(request.QuestionId, couple.Id, cancellationToken);
        return Result.Success(mapper.Map<IReadOnlyList<AnswerDto>>(answers));
    }
}
