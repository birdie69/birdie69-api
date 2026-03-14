using System.Text.Json;
using Birdie69.Application.Common.Interfaces;
using Birdie69.Domain.Common;
using Birdie69.Domain.Entities;
using Birdie69.Domain.Interfaces;
using MediatR;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;

namespace Birdie69.Application.Features.Questions.Queries.GetTodayQuestion;

public sealed class GetTodayQuestionQueryHandler(
    ICmsService cmsService,
    IQuestionRepository questionRepository,
    IUnitOfWork unitOfWork,
    IDistributedCache cache,
    ILogger<GetTodayQuestionQueryHandler> logger)
    : IRequestHandler<GetTodayQuestionQuery, Result<QuestionDto>>
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    public async Task<Result<QuestionDto>> Handle(
        GetTodayQuestionQuery request,
        CancellationToken cancellationToken)
    {
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var cacheKey = $"question:today:{today:yyyy-MM-dd}";

        var cached = await cache.GetStringAsync(cacheKey, cancellationToken);
        if (cached is not null)
        {
            logger.LogDebug("Cache hit for {CacheKey}", cacheKey);
            var cachedDto = JsonSerializer.Deserialize<QuestionDto>(cached, JsonOptions);
            if (cachedDto is not null)
                return Result.Success(cachedDto);
        }

        var cmsDto = await cmsService.GetTodayQuestionAsync(today, cancellationToken);
        if (cmsDto is null)
            return Result.Failure<QuestionDto>(Error.NotFound("Question", today));

        var existingByDate = await questionRepository.GetByScheduledDateAsync(today, cancellationToken);
        if (existingByDate is not null)
        {
            var dto = ToDto(existingByDate);
            await SetCacheAsync(cacheKey, dto, cancellationToken);
            return Result.Success(dto);
        }

        Question question;
        try
        {
            question = Question.Create(
                Guid.NewGuid(),
                cmsDto.DocumentId,
                cmsDto.Title,
                cmsDto.Body,
                cmsDto.Category,
                today,
                cmsDto.Tags);

            await questionRepository.AddAsync(question, cancellationToken);
            await unitOfWork.SaveChangesAsync(cancellationToken);
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            // Race condition guard: a concurrent request already inserted the question.
            // Fall back to the existing record identified by the CMS document id.
            logger.LogWarning(ex, "Insert failed for question {DocumentId} on {Date} — fetching existing record", cmsDto.DocumentId, today);
            var fallback = await questionRepository.GetByExternalIdAsync(cmsDto.DocumentId, cancellationToken);
            if (fallback is null)
                return Result.Failure<QuestionDto>(Error.NotFound("Question", today));
            question = fallback;
        }

        var result = ToDto(question);
        await SetCacheAsync(cacheKey, result, cancellationToken);
        return Result.Success(result);
    }

    private async Task SetCacheAsync(string key, QuestionDto dto, CancellationToken ct)
    {
        var now = DateTime.UtcNow;
        var midnight = now.Date.AddDays(1);
        var ttl = (int)(midnight - now).TotalSeconds;
        if (ttl <= 0) return;

        await cache.SetStringAsync(
            key,
            JsonSerializer.Serialize(dto, JsonOptions),
            new DistributedCacheEntryOptions { AbsoluteExpirationRelativeToNow = TimeSpan.FromSeconds(ttl) },
            ct);
    }

    private static QuestionDto ToDto(Question q) => new(
        Id: q.Id,
        DocumentId: q.ExternalDocumentId,
        Title: q.Title,
        Body: q.Body,
        Category: q.Category,
        ScheduledDate: q.ScheduledDate,
        Tags: q.Tags);
}
