using System.Net.Http.Json;
using System.Text.Json;
using Birdie69.Application.Common.Interfaces;
using Birdie69.Application.Features.Questions.Queries.GetTodayQuestion;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;

namespace Birdie69.Infrastructure.Cms;

/// <summary>
/// Fetches daily questions from Strapi CMS with Redis-backed caching.
/// Cache TTL is set to expire at midnight UTC so each day gets a fresh question.
/// </summary>
public sealed class CmsService(
    HttpClient httpClient,
    IDistributedCache cache,
    ILogger<CmsService> logger)
    : ICmsService
{
    private static readonly JsonSerializerOptions JsonOptions =
        new(JsonSerializerDefaults.Web);

    public async Task<QuestionDto?> GetTodayQuestionAsync(DateOnly date, CancellationToken ct = default)
    {
        var cacheKey = $"question:today:{date:yyyy-MM-dd}";

        var cached = await cache.GetStringAsync(cacheKey, ct);
        if (cached is not null)
        {
            logger.LogDebug("Cache hit for {CacheKey}", cacheKey);
            return JsonSerializer.Deserialize<QuestionDto>(cached, JsonOptions);
        }

        var url = $"api/questions" +
                  $"?filters[scheduledDate][$eq]={date:yyyy-MM-dd}" +
                  $"&filters[isActive][$eq]=true" +
                  $"&publicationState=live";

        logger.LogInformation("Fetching today's question from Strapi: GET {Url}", url);

        StrapiListResponse<StrapiQuestion>? response;
        try
        {
            response = await httpClient.GetFromJsonAsync<StrapiListResponse<StrapiQuestion>>(
                url, JsonOptions, ct);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to fetch question from Strapi (url={Url})", url);
            return null;
        }

        var item = response?.Data?.FirstOrDefault();
        if (item is null)
        {
            logger.LogWarning("No question found in Strapi for date {Date}", date);
            return null;
        }

        var dto = new QuestionDto(
            DocumentId: item.DocumentId,
            Title: item.Title,
            Body: item.Body,
            Category: item.Category ?? string.Empty,
            ScheduledDate: item.ScheduledDate,
            Tags: item.Tags ?? []);

        var ttlSeconds = SecondsUntilMidnightUtc();
        if (ttlSeconds > 0)
        {
            await cache.SetStringAsync(
                cacheKey,
                JsonSerializer.Serialize(dto, JsonOptions),
                new DistributedCacheEntryOptions
                {
                    AbsoluteExpirationRelativeToNow = TimeSpan.FromSeconds(ttlSeconds)
                },
                ct);
        }

        return dto;
    }

    private static int SecondsUntilMidnightUtc()
    {
        var now = DateTime.UtcNow;
        var midnight = now.Date.AddDays(1);
        return (int)(midnight - now).TotalSeconds;
    }
}
