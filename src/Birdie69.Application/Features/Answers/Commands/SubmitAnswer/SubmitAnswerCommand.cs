using Birdie69.Domain.Common;
using MediatR;

namespace Birdie69.Application.Features.Answers.Commands.SubmitAnswer;

public sealed record SubmitAnswerCommand(
    Guid QuestionId,
    string Text) : IRequest<Result<Guid>>;
