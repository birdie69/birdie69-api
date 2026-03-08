using Birdie69.Domain.Common;
using MediatR;

namespace Birdie69.Application.Features.Questions.Queries.GetTodayQuestion;

public sealed record GetTodayQuestionQuery : IRequest<Result<QuestionDto>>;
