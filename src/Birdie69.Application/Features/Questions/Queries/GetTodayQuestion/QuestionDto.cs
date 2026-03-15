namespace Birdie69.Application.Features.Questions.Queries.GetTodayQuestion;

public sealed record QuestionDto(
    Guid Id,
    string DocumentId,
    string Title,
    string Body,
    string Category,
    DateOnly ScheduledDate,
    IReadOnlyList<string> Tags);
