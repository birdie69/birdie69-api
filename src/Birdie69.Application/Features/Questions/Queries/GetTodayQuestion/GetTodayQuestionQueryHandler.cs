using AutoMapper;
using Birdie69.Domain.Common;
using Birdie69.Domain.Interfaces;
using MediatR;

namespace Birdie69.Application.Features.Questions.Queries.GetTodayQuestion;

public sealed class GetTodayQuestionQueryHandler(
    IQuestionRepository questionRepository,
    IMapper mapper)
    : IRequestHandler<GetTodayQuestionQuery, Result<QuestionDto>>
{
    public async Task<Result<QuestionDto>> Handle(
        GetTodayQuestionQuery request,
        CancellationToken cancellationToken)
    {
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var question = await questionRepository.GetByScheduledDateAsync(today, cancellationToken);

        if (question is null)
            return Result.Failure<QuestionDto>(Error.NotFound("Question", today));

        return mapper.Map<QuestionDto>(question);
    }
}
