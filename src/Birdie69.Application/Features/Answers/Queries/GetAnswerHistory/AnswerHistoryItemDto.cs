using Birdie69.Application.Features.Answers.Queries.GetAnswers;

namespace Birdie69.Application.Features.Answers.Queries.GetAnswerHistory;

public sealed record AnswerHistoryItemDto(
    Guid QuestionId,
    string QuestionTitle,
    string QuestionBody,
    string ScheduledDate,
    AnswerDto? MyAnswer,
    AnswerDto? PartnerAnswer,
    bool IsRevealed);
