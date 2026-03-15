using Birdie69.Domain.Common;
using MediatR;

namespace Birdie69.Application.Features.Answers.Queries.GetAnswers;

/// <summary>
/// Returns the reveal state for both partners' answers on a question.
/// Always succeeds (HTTP 200) as long as the caller is in an active couple.
/// IsRevealed is only true when both partners have submitted.
/// </summary>
public sealed record GetAnswersQuery(Guid QuestionId) : IRequest<Result<AnswerRevealDto>>;
