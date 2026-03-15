namespace Birdie69.Application.Features.Answers.Queries.GetAnswers;

public sealed record AnswerRevealDto(
    bool IsRevealed,
    AnswerDto? MyAnswer,
    AnswerDto? PartnerAnswer);
