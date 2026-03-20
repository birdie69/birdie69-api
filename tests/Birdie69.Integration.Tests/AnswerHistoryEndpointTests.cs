using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Security.Claims;
using System.Text;
using System.Text.Json;
using Birdie69.Application.Features.Questions.Queries.GetTodayQuestion;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using Moq;

namespace Birdie69.Integration.Tests;

/// <summary>
/// End-to-end tests for GET /v1/answers/history.
/// </summary>
public sealed class AnswerHistoryEndpointTests(WebAppFactory factory)
    : IClassFixture<WebAppFactory>
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    private HttpClient ClientFor(string sub)
    {
        var client = factory.CreateClient();
        client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", CreateDevJwt(sub));
        return client;
    }

    private static string CreateDevJwt(string sub)
    {
        var key = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes("birdie69-dev-only-key-not-used-in-prod!"));
        var token = new JwtSecurityToken(
            claims: [new Claim(ClaimTypes.NameIdentifier, sub)],
            signingCredentials: new SigningCredentials(key, SecurityAlgorithms.HmacSha256));
        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    private async Task<(Guid QuestionId, HttpClient Alice, HttpClient Bob)> SetupCoupleAsync(string runId)
    {
        var aliceSub = $"hist-alice-{runId}";
        var bobSub   = $"hist-bob-{runId}";
        var alice = ClientFor(aliceSub);
        var bob   = ClientFor(bobSub);

        var docId = $"hist-doc-{runId}";
        factory.CmsServiceMock
            .Setup(x => x.GetTodayQuestionAsync(It.IsAny<DateOnly>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new QuestionDto(
                Id: Guid.Empty,
                DocumentId: docId,
                Title: $"History question {runId}",
                Body: "Test body.",
                Category: "fun",
                ScheduledDate: DateOnly.FromDateTime(DateTime.UtcNow),
                Tags: []));

        using var scope = factory.Services.CreateScope();
        var cache = scope.ServiceProvider.GetRequiredService<IDistributedCache>();
        await cache.RemoveAsync($"question:today:{DateTime.UtcNow:yyyy-MM-dd}");

        await alice.PutAsJsonAsync("/v1/users/me", new { displayName = $"HistAlice-{runId}", avatarUrl = (string?)null });
        await bob.PutAsJsonAsync("/v1/users/me", new { displayName = $"HistBob-{runId}", avatarUrl = (string?)null });

        var createResp = await alice.PostAsync("/v1/couples", null);
        var inviteCode = JsonSerializer.Deserialize<JsonElement>(
            await createResp.Content.ReadAsStringAsync(), JsonOptions)
            .GetProperty("inviteCode").GetString()!;
        await bob.PostAsJsonAsync("/v1/couples/join", new { inviteCode });

        var qResp = await alice.GetAsync("/v1/questions/today");
        var questionId = Guid.Parse(
            JsonSerializer.Deserialize<JsonElement>(
                await qResp.Content.ReadAsStringAsync(), JsonOptions)
            .GetProperty("id").GetString()!);

        return (questionId, alice, bob);
    }

    [Fact]
    public async Task GetHistory_NoAnswers_ReturnsEmptyList()
    {
        var (_, alice, _) = await SetupCoupleAsync("hist-empty");

        var response = await alice.GetAsync("/v1/answers/history?page=1&pageSize=20");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var json = JsonSerializer.Deserialize<JsonElement>(
            await response.Content.ReadAsStringAsync(), JsonOptions);
        json.GetProperty("items").GetArrayLength().Should().Be(0);
        json.GetProperty("totalCount").GetInt32().Should().Be(0);
        json.GetProperty("page").GetInt32().Should().Be(1);
        json.GetProperty("pageSize").GetInt32().Should().Be(20);
    }

    [Fact]
    public async Task GetHistory_WithAnswers_ReturnsItems()
    {
        var (questionId, alice, _) = await SetupCoupleAsync("hist-items");

        await alice.PostAsJsonAsync("/v1/answers", new { questionId, text = "Alice answer" });

        var response = await alice.GetAsync("/v1/answers/history?page=1&pageSize=20");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var json = JsonSerializer.Deserialize<JsonElement>(
            await response.Content.ReadAsStringAsync(), JsonOptions);
        json.GetProperty("totalCount").GetInt32().Should().Be(1);
        var items = json.GetProperty("items");
        items.GetArrayLength().Should().Be(1);
        items[0].GetProperty("questionId").GetString().Should().Be(questionId.ToString());
        items[0].GetProperty("myAnswer").ValueKind.Should().NotBe(JsonValueKind.Null);
        items[0].GetProperty("partnerAnswer").ValueKind.Should().Be(JsonValueKind.Null);
        items[0].GetProperty("isRevealed").GetBoolean().Should().BeFalse();
    }
}
