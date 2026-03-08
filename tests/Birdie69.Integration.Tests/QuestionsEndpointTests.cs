using FluentAssertions;
using System.Net;
using System.Net.Http.Headers;

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

    [Fact]
    public async Task GetToday_WithDevToken_DoesNotReturn401()
    {
        // Verifies the dev auth bypass works: any non-JWT Bearer value
        // must be accepted (not 401) so Swagger works without B2C configured.
        _client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", "dev");

        var response = await _client.GetAsync("/v1/questions/today");

        response.StatusCode.Should().NotBe(HttpStatusCode.Unauthorized);
    }
}
