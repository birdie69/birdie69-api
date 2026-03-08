namespace Birdie69.Application.Features.Answers.Queries.GetAnswers;

public sealed record AnswerDto(
    Guid Id,
    Guid UserId,
    string Text,
    DateTime SubmittedAt);
