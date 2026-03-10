using Birdie69.Application.Features.Questions.Queries.GetTodayQuestion;
using FluentAssertions;
using Moq;
using System.Net;
using System.Net.Http.Headers;

namespace Birdie69.Integration.Tests;

public sealed class QuestionsEndpointTests(WebAppFactory factory)
    : IClassFixture<WebAppFactory>
{
    [Fact]
    public async Task GetToday_WithoutAuth_Returns401()
    {
        var client = factory.CreateClient();

        var response = await client.GetAsync("/v1/questions/today");

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task GetToday_WithAuth_WhenCmsReturnsNull_Returns404()
    {
        // Re-establish null setup: tests share the same WebAppFactory instance (IClassFixture),
        // so a previous test that configured the mock to return a question would bleed over.
        factory.CmsServiceMock
            .Setup(x => x.GetTodayQuestionAsync(It.IsAny<DateOnly>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((QuestionDto?)null);

        var client = factory.CreateClient();
        client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", "dev-test-token");

        var response = await client.GetAsync("/v1/questions/today");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task GetToday_WithAuth_WhenCmsReturnsQuestion_Returns200()
    {
        factory.CmsServiceMock
            .Setup(x => x.GetTodayQuestionAsync(It.IsAny<DateOnly>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new QuestionDto(
                DocumentId: "doc-abc",
                Title: "Integration test question",
                Body: "Body text for integration test.",
                Category: "fun",
                ScheduledDate: DateOnly.FromDateTime(DateTime.UtcNow),
                Tags: ["integration"]));

        var client = factory.CreateClient();
        client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", "dev-test-token");

        var response = await client.GetAsync("/v1/questions/today");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await response.Content.ReadAsStringAsync();
        body.Should().Contain("Integration test question");
    }
}
