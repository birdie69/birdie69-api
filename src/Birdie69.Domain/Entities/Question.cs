using Birdie69.Domain.Common;

namespace Birdie69.Domain.Entities;

/// <summary>
/// A read-only snapshot of a question sourced from Strapi CMS.
/// Questions are fetched and cached here; birdie69-api never mutates them.
/// </summary>
public sealed class Question : Entity
{
    private readonly List<string> _tags = [];

    public string ExternalId { get; private set; } = string.Empty;
    public string Text { get; private set; } = string.Empty;
    public DateOnly ScheduledDate { get; private set; }
    public IReadOnlyList<string> Tags => _tags.AsReadOnly();
    public DateTime SyncedAt { get; private set; }

    private Question() { }

    public static Question Create(
        Guid id,
        string externalId,
        string text,
        DateOnly scheduledDate,
        IEnumerable<string>? tags = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(externalId);
        ArgumentException.ThrowIfNullOrWhiteSpace(text);

        var question = new Question
        {
            Id = id,
            ExternalId = externalId,
            Text = text,
            ScheduledDate = scheduledDate,
            SyncedAt = DateTime.UtcNow,
        };

        if (tags is not null)
            question._tags.AddRange(tags);

        return question;
    }
}
