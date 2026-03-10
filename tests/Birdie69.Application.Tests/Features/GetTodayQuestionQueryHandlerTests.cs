using Birdie69.Application.Common.Interfaces;
using Birdie69.Application.Features.Questions.Queries.GetTodayQuestion;
using FluentAssertions;
using Moq;

namespace Birdie69.Application.Tests.Features;

public sealed class GetTodayQuestionQueryHandlerTests
{
    private readonly Mock<ICmsService> _cmsService = new();

    private GetTodayQuestionQueryHandler CreateHandler() =>
        new(_cmsService.Object);

    [Fact]
    public async Task Handle_WhenCmsReturnsQuestion_ReturnsSuccess()
    {
        var expected = new QuestionDto(
            DocumentId: "doc123",
            Title: "What's your favourite memory together?",
            Body: "Take a moment to think back...",
            Category: "memory",
            ScheduledDate: DateOnly.FromDateTime(DateTime.UtcNow),
            Tags: ["memory", "nostalgia"]);

        _cmsService
            .Setup(x => x.GetTodayQuestionAsync(It.IsAny<DateOnly>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(expected);

        var result = await CreateHandler().Handle(new GetTodayQuestionQuery(), default);

        result.IsSuccess.Should().BeTrue();
        result.Value.DocumentId.Should().Be("doc123");
        result.Value.Title.Should().Be("What's your favourite memory together?");
        result.Value.Category.Should().Be("memory");
    }

    [Fact]
    public async Task Handle_WhenCmsReturnsNull_ReturnsNotFound()
    {
        _cmsService
            .Setup(x => x.GetTodayQuestionAsync(It.IsAny<DateOnly>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((QuestionDto?)null);

        var result = await CreateHandler().Handle(new GetTodayQuestionQuery(), default);

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Contain("NotFound");
    }

    [Fact]
    public async Task Handle_PassesTodayDateToCmsService()
    {
        DateOnly capturedDate = default;
        _cmsService
            .Setup(x => x.GetTodayQuestionAsync(It.IsAny<DateOnly>(), It.IsAny<CancellationToken>()))
            .Callback<DateOnly, CancellationToken>((d, _) => capturedDate = d)
            .ReturnsAsync((QuestionDto?)null);

        await CreateHandler().Handle(new GetTodayQuestionQuery(), default);

        capturedDate.Should().Be(DateOnly.FromDateTime(DateTime.UtcNow));
    }
}
