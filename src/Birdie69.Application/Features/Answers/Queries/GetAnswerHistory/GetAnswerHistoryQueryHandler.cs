using AutoMapper;
using Birdie69.Application.Common.Interfaces;
using Birdie69.Application.Features.Answers.Queries.GetAnswers;
using Birdie69.Domain.Common;
using Birdie69.Domain.Entities;
using Birdie69.Domain.Interfaces;
using MediatR;

namespace Birdie69.Application.Features.Answers.Queries.GetAnswerHistory;

public sealed class GetAnswerHistoryQueryHandler(
    IAnswerRepository answerRepository,
    ICoupleRepository coupleRepository,
    IUserRepository userRepository,
    ICurrentUser currentUser,
    IMapper mapper)
    : IRequestHandler<GetAnswerHistoryQuery, Result<AnswerHistoryPageDto>>
{
    public async Task<Result<AnswerHistoryPageDto>> Handle(
        GetAnswerHistoryQuery request,
        CancellationToken cancellationToken)
    {
        var user = await userRepository.GetByExternalIdAsync(currentUser.ExternalId, cancellationToken);
        if (user is null)
            return Result.Failure<AnswerHistoryPageDto>(Error.NotFound("User", currentUser.ExternalId));

        var couple = await coupleRepository.GetActiveByUserIdAsync(user.Id, cancellationToken);
        if (couple is null)
            return Result.Failure<AnswerHistoryPageDto>(
                Error.Unauthorized("Answer.NoCouple", "You must be in an active couple to view history."));

        var (items, totalCount) = await answerRepository.GetHistoryByCoupleAsync(
            couple.Id, user.Id, request.Page, request.PageSize, cancellationToken);

        var dtos = items.Select(i => new AnswerHistoryItemDto(
            QuestionId: i.Question.Id,
            QuestionTitle: i.Question.Title,
            QuestionBody: i.Question.Body,
            ScheduledDate: i.Question.ScheduledDate.ToString("yyyy-MM-dd"),
            MyAnswer: i.MyAnswer is not null ? mapper.Map<AnswerDto>(i.MyAnswer) : null,
            PartnerAnswer: i.PartnerAnswer is not null ? mapper.Map<AnswerDto>(i.PartnerAnswer) : null,
            IsRevealed: i.MyAnswer is not null && i.PartnerAnswer is not null))
            .ToList();

        return Result.Success(new AnswerHistoryPageDto(dtos, totalCount, request.Page, request.PageSize));
    }
}
