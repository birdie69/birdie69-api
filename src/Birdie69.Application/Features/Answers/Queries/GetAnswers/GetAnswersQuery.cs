using Birdie69.Domain.Common;
using MediatR;

namespace Birdie69.Application.Features.Answers.Queries.GetAnswers;

/// <summary>
/// Returns both partners' answers for a question.
/// Only succeeds if both have submitted (business rule: no peeking before partner answers).
/// </summary>
public sealed record GetAnswersQuery(Guid QuestionId) : IRequest<Result<IReadOnlyList<AnswerDto>>>;
