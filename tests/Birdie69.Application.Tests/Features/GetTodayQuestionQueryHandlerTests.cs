using System.Text;
using System.Text.Json;
using Birdie69.Application.Features.Questions.Queries.GetTodayQuestion;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging.Abstractions;

namespace Birdie69.Application.Tests.Features;

public sealed class GetTodayQuestionQueryHandlerTests
{
    private readonly Mock<ICmsService> _cmsService = new();
    private readonly Mock<IQuestionRepository> _questionRepo = new();
    private readonly Mock<IUnitOfWork> _uow = new();
    private readonly Mock<IDistributedCache> _cache = new();

    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    private GetTodayQuestionQueryHandler CreateHandler() => new(
        _cmsService.Object,
        _questionRepo.Object,
        _uow.Object,
        _cache.Object,
        NullLogger<GetTodayQuestionQueryHandler>.Instance);

    private static QuestionDto MakeCmsDto(string docId = "doc-1") => new(
        Id: Guid.Empty,
        DocumentId: docId,
        Title: "Test question?",
        Body: "Think about it.",
        Category: "fun",
        ScheduledDate: DateOnly.FromDateTime(DateTime.UtcNow),
        Tags: ["tag1"]);

    private static Question MakeQuestion(Guid? id = null, string docId = "doc-1")
        => Question.Create(
            id ?? Guid.NewGuid(),
            docId,
            "Test question?",
            "Think about it.",
            "fun",
            DateOnly.FromDateTime(DateTime.UtcNow),
            ["tag1"]);

    private void SetupCacheHit(QuestionDto dto)
    {
        var json = JsonSerializer.Serialize(dto, JsonOptions);
        _cache
            .Setup(x => x.GetAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Encoding.UTF8.GetBytes(json));
    }

    private void SetupCacheMiss()
    {
        _cache
            .Setup(x => x.GetAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((byte[]?)null);
    }

    [Fact]
    public async Task Handle_CacheHit_ReturnsCachedDtoWithoutDbCalls()
    {
        var cachedId = Guid.NewGuid();
        var cachedDto = MakeCmsDto() with { Id = cachedId };
        SetupCacheHit(cachedDto);

        var result = await CreateHandler().Handle(new GetTodayQuestionQuery(), default);

        result.IsSuccess.Should().BeTrue();
        result.Value.Id.Should().Be(cachedId);
        _questionRepo.Verify(x => x.GetByScheduledDateAsync(It.IsAny<DateOnly>(), It.IsAny<CancellationToken>()), Times.Never);
        _cmsService.Verify(x => x.GetTodayQuestionAsync(It.IsAny<DateOnly>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_CacheMiss_QuestionNotInDb_InsertsAndCachesAndReturnsWithGuid()
    {
        SetupCacheMiss();
        _cmsService
            .Setup(x => x.GetTodayQuestionAsync(It.IsAny<DateOnly>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(MakeCmsDto());
        _questionRepo
            .Setup(x => x.GetByScheduledDateAsync(It.IsAny<DateOnly>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Question?)null);
        _uow.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

        var result = await CreateHandler().Handle(new GetTodayQuestionQuery(), default);

        result.IsSuccess.Should().BeTrue();
        result.Value.Id.Should().NotBe(Guid.Empty);
        result.Value.DocumentId.Should().Be("doc-1");
        _questionRepo.Verify(x => x.AddAsync(It.IsAny<Question>(), It.IsAny<CancellationToken>()), Times.Once);
        _uow.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        _cache.Verify(x => x.SetAsync(It.IsAny<string>(), It.IsAny<byte[]>(), It.IsAny<DistributedCacheEntryOptions>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_CacheMiss_QuestionAlreadyInDb_ReturnsExistingWithoutInsert()
    {
        var existing = MakeQuestion(Guid.NewGuid());
        SetupCacheMiss();
        _cmsService
            .Setup(x => x.GetTodayQuestionAsync(It.IsAny<DateOnly>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(MakeCmsDto());
        _questionRepo
            .Setup(x => x.GetByScheduledDateAsync(It.IsAny<DateOnly>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(existing);

        var result = await CreateHandler().Handle(new GetTodayQuestionQuery(), default);

        result.IsSuccess.Should().BeTrue();
        result.Value.Id.Should().Be(existing.Id);
        _questionRepo.Verify(x => x.AddAsync(It.IsAny<Question>(), It.IsAny<CancellationToken>()), Times.Never);
        _uow.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_CmsReturnsNull_ReturnsNotFound()
    {
        SetupCacheMiss();
        _cmsService
            .Setup(x => x.GetTodayQuestionAsync(It.IsAny<DateOnly>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((QuestionDto?)null);

        var result = await CreateHandler().Handle(new GetTodayQuestionQuery(), default);

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Contain("NotFound");
    }

    [Fact]
    public async Task Handle_SaveChangesThrows_FallsBackToGetByExternalId()
    {
        var fallback = MakeQuestion(Guid.NewGuid(), "doc-1");
        SetupCacheMiss();
        _cmsService
            .Setup(x => x.GetTodayQuestionAsync(It.IsAny<DateOnly>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(MakeCmsDto());
        _questionRepo
            .Setup(x => x.GetByScheduledDateAsync(It.IsAny<DateOnly>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Question?)null);
        _uow.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("Simulated unique constraint violation"));
        _questionRepo
            .Setup(x => x.GetByExternalIdAsync("doc-1", It.IsAny<CancellationToken>()))
            .ReturnsAsync(fallback);

        var result = await CreateHandler().Handle(new GetTodayQuestionQuery(), default);

        result.IsSuccess.Should().BeTrue();
        result.Value.Id.Should().Be(fallback.Id);
    }
}
