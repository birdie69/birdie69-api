using Birdie69.Domain.Common;
using MediatR;

namespace Birdie69.Application.Features.Answers.Queries.GetAnswerHistory;

public sealed record GetAnswerHistoryQuery(int Page, int PageSize)
    : IRequest<Result<AnswerHistoryPageDto>>;
