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
    : IRequestHandler<GetAnswersQuery, Result<AnswerRevealDto>>
{
    public async Task<Result<AnswerRevealDto>> Handle(
        GetAnswersQuery request,
        CancellationToken cancellationToken)
    {
        var user = await userRepository.GetByExternalIdAsync(currentUser.ExternalId, cancellationToken);
        if (user is null)
            return Result.Failure<AnswerRevealDto>(Error.NotFound("User", currentUser.ExternalId));

        var couple = await coupleRepository.GetActiveByUserIdAsync(user.Id, cancellationToken);
        if (couple is null)
            return Result.Failure<AnswerRevealDto>(Error.Conflict("Answer.NoCouple", "You must be in an active couple."));

        var myAnswer = await answerRepository.GetByUserAndQuestionAsync(user.Id, request.QuestionId, cancellationToken);
        var bothAnswered = await answerRepository.BothPartnersAnsweredAsync(request.QuestionId, couple.Id, cancellationToken);

        if (!bothAnswered)
        {
            return Result.Success(new AnswerRevealDto(
                IsRevealed: false,
                MyAnswer: myAnswer is not null ? mapper.Map<AnswerDto>(myAnswer) : null,
                PartnerAnswer: null));
        }

        var allAnswers = await answerRepository.GetByQuestionAndCoupleAsync(request.QuestionId, couple.Id, cancellationToken);
        var partnerAnswer = allAnswers.FirstOrDefault(a => a.UserId != user.Id);

        return Result.Success(new AnswerRevealDto(
            IsRevealed: true,
            MyAnswer: myAnswer is not null ? mapper.Map<AnswerDto>(myAnswer) : null,
            PartnerAnswer: partnerAnswer is not null ? mapper.Map<AnswerDto>(partnerAnswer) : null));
    }
}
