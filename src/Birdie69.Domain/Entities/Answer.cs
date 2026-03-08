using Birdie69.Domain.Common;
using Birdie69.Domain.Events;

namespace Birdie69.Domain.Entities;

/// <summary>
/// A user's answer to a daily question.
/// Business rule: an answer can only be submitted once per question per user.
/// Answers are only readable after both partners have submitted (AnswerReveal).
/// </summary>
public sealed class Answer : AuditableEntity
{
    public Guid UserId { get; private set; }
    public Guid QuestionId { get; private set; }
    public Guid CoupleId { get; private set; }
    public string Text { get; private set; } = string.Empty;
    public DateTime SubmittedAt { get; private set; }
    public bool IsRevealed { get; private set; }

    private Answer() { }

    public static Answer Submit(
        Guid id,
        Guid userId,
        Guid questionId,
        Guid coupleId,
        string text)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(text);

        var answer = new Answer
        {
            Id = id,
            UserId = userId,
            QuestionId = questionId,
            CoupleId = coupleId,
            Text = text,
            SubmittedAt = DateTime.UtcNow,
        };

        answer.RaiseDomainEvent(new AnswerSubmittedEvent(id, userId, questionId, coupleId));
        return answer;
    }

    public void Reveal()
    {
        if (!IsRevealed)
        {
            IsRevealed = true;
            RaiseDomainEvent(new AnswerRevealedEvent(Id, UserId, QuestionId, CoupleId));
        }
    }
}
