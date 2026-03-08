using FluentAssertions;
using System.Net;

namespace Birdie69.Integration.Tests;

public sealed class QuestionsEndpointTests(WebAppFactory factory)
    : IClassFixture<WebAppFactory>
{
    private readonly HttpClient _client = factory.CreateClient();

    [Fact]
    public async Task GetToday_WithoutAuth_Returns401()
    {
        var response = await _client.GetAsync("/v1/questions/today");
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }
}
