using Birdie69.Domain.Common;

namespace Birdie69.Domain.Entities;

/// <summary>
/// A read-only snapshot of a question sourced from Strapi CMS.
/// Questions are upserted from CMS on first access; the local Guid is used as FK for Answers.
/// </summary>
public sealed class Question : Entity
{
    private readonly List<string> _tags = [];

    public string ExternalDocumentId { get; private set; } = string.Empty;
    public string Title { get; private set; } = string.Empty;
    public string Body { get; private set; } = string.Empty;
    public string Category { get; private set; } = string.Empty;
    public DateOnly ScheduledDate { get; private set; }
    public IReadOnlyList<string> Tags => _tags.AsReadOnly();
    public DateTime CreatedAt { get; private set; }

    private Question() { }

    public static Question Create(
        Guid id,
        string externalDocumentId,
        string title,
        string body,
        string category,
        DateOnly scheduledDate,
        IEnumerable<string>? tags = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(externalDocumentId);
        ArgumentException.ThrowIfNullOrWhiteSpace(title);

        var question = new Question
        {
            Id = id,
            ExternalDocumentId = externalDocumentId,
            Title = title,
            Body = body,
            Category = category,
            ScheduledDate = scheduledDate,
            CreatedAt = DateTime.UtcNow,
        };

        if (tags is not null)
            question._tags.AddRange(tags);

        return question;
    }
}
