using System.Text.Json.Serialization;

namespace Birdie69.Infrastructure.Cms;

/// <summary>
/// Strapi v5 REST list response envelope.
/// The "data" array contains flat attribute objects (no nested "attributes" wrapper like v4).
/// </summary>
public sealed class StrapiListResponse<T>
{
    [JsonPropertyName("data")]
    public List<T>? Data { get; init; }
}

/// <summary>
/// Maps the Strapi v5 Question content type REST response to a C# model.
/// Field names match the Strapi schema exactly (camelCase as returned by the API).
/// </summary>
public sealed class StrapiQuestion
{
    [JsonPropertyName("documentId")]
    public string DocumentId { get; init; } = string.Empty;

    [JsonPropertyName("title")]
    public string Title { get; init; } = string.Empty;

    [JsonPropertyName("body")]
    public string Body { get; init; } = string.Empty;

    [JsonPropertyName("category")]
    public string? Category { get; init; }

    [JsonPropertyName("scheduledDate")]
    public DateOnly ScheduledDate { get; init; }

    [JsonPropertyName("tags")]
    public List<string>? Tags { get; init; }

    [JsonPropertyName("isActive")]
    public bool IsActive { get; init; }
}
