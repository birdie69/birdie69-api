using Birdie69.Application.Common.Interfaces;
using Birdie69.Domain.Common;
using MediatR;

namespace Birdie69.Application.Features.Questions.Queries.GetTodayQuestion;

public sealed class GetTodayQuestionQueryHandler(ICmsService cmsService)
    : IRequestHandler<GetTodayQuestionQuery, Result<QuestionDto>>
{
    public async Task<Result<QuestionDto>> Handle(
        GetTodayQuestionQuery request,
        CancellationToken cancellationToken)
    {
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var question = await cmsService.GetTodayQuestionAsync(today, cancellationToken);

        if (question is null)
            return Result.Failure<QuestionDto>(Error.NotFound("Question", today));

        return Result.Success(question);
    }
}
