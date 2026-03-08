namespace Birdie69.Application.Features.Questions.Queries.GetTodayQuestion;

public sealed record QuestionDto(
    Guid Id,
    string Text,
    DateOnly ScheduledDate,
    IReadOnlyList<string> Tags);
