namespace Birdie69.Application.Features.Answers.Queries.GetAnswerHistory;

public sealed record AnswerHistoryPageDto(
    IReadOnlyList<AnswerHistoryItemDto> Items,
    int TotalCount,
    int Page,
    int PageSize);
