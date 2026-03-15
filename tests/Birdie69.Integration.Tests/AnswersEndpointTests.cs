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
/// End-to-end tests for POST /v1/answers and GET /v1/answers/{questionId}.
/// Each test creates uniquely-subbed users so their couples are isolated in the shared DB.
/// </summary>
public sealed class AnswersEndpointTests(WebAppFactory factory)
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

    /// <summary>
    /// Registers two users, forms a couple, and fetches today's question.
    /// Each call uses a unique runId so couples don't collide across tests.
    /// Returns (questionId, aliceClient, bobClient).
    /// </summary>
    private async Task<(Guid QuestionId, HttpClient Alice, HttpClient Bob)> SetupAsync(string runId)
    {
        var aliceSub = $"alice-{runId}";
        var bobSub   = $"bob-{runId}";
        var alice = ClientFor(aliceSub);
        var bob   = ClientFor(bobSub);

        // Configure CMS mock with a document id unique per run so a fresh row is inserted
        var docId = $"doc-{runId}";
        factory.CmsServiceMock
            .Setup(x => x.GetTodayQuestionAsync(It.IsAny<DateOnly>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new QuestionDto(
                Id: Guid.Empty,
                DocumentId: docId,
                Title: $"Question for run {runId}",
                Body: "Test body.",
                Category: "test",
                ScheduledDate: DateOnly.FromDateTime(DateTime.UtcNow),
                Tags: []));

        // Evict cache so handler inserts (or finds existing by scheduled date on same-date collision)
        using var scope = factory.Services.CreateScope();
        var cache = scope.ServiceProvider.GetRequiredService<IDistributedCache>();
        await cache.RemoveAsync($"question:today:{DateTime.UtcNow:yyyy-MM-dd}");

        // Register users
        await alice.PutAsJsonAsync("/v1/users/me", new { displayName = $"Alice-{runId}", avatarUrl = (string?)null });
        await bob.PutAsJsonAsync("/v1/users/me", new { displayName = $"Bob-{runId}", avatarUrl = (string?)null });

        // Create couple (Alice invites)
        var createResp = await alice.PostAsync("/v1/couples", null);
        createResp.StatusCode.Should().Be(HttpStatusCode.Created);
        var inviteCode = JsonSerializer.Deserialize<JsonElement>(
            await createResp.Content.ReadAsStringAsync(), JsonOptions)
            .GetProperty("inviteCode").GetString()!;

        // Bob joins
        var joinResp = await bob.PostAsJsonAsync("/v1/couples/join", new { inviteCode });
        joinResp.StatusCode.Should().Be(HttpStatusCode.OK);

        // Get today's question (upserts into DB)
        var qResp = await alice.GetAsync("/v1/questions/today");
        qResp.StatusCode.Should().Be(HttpStatusCode.OK);
        var questionId = Guid.Parse(
            JsonSerializer.Deserialize<JsonElement>(
                await qResp.Content.ReadAsStringAsync(), JsonOptions)
            .GetProperty("id").GetString()!);

        return (questionId, alice, bob);
    }

    [Fact]
    public async Task SubmitAnswer_WithDevToken_Returns201WithId()
    {
        var (questionId, alice, _) = await SetupAsync("submit-201");

        var response = await alice.PostAsJsonAsync("/v1/answers", new
        {
            questionId,
            text = "Alice's answer"
        });

        response.StatusCode.Should().Be(HttpStatusCode.Created);
        var body = await response.Content.ReadAsStringAsync();
        body.Should().Contain("id");
    }

    [Fact]
    public async Task SubmitAnswer_Duplicate_Returns409()
    {
        var (questionId, alice, _) = await SetupAsync("submit-dup");

        await alice.PostAsJsonAsync("/v1/answers", new { questionId, text = "First" });
        var response = await alice.PostAsJsonAsync("/v1/answers", new { questionId, text = "Duplicate" });

        response.StatusCode.Should().Be(HttpStatusCode.Conflict);
    }

    [Fact]
    public async Task GetAnswers_OnlyCallerAnswered_IsRevealedFalse()
    {
        var (questionId, alice, _) = await SetupAsync("get-one");

        await alice.PostAsJsonAsync("/v1/answers", new { questionId, text = "Alice only" });

        var response = await alice.GetAsync($"/v1/answers/{questionId}");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var json = JsonSerializer.Deserialize<JsonElement>(
            await response.Content.ReadAsStringAsync(), JsonOptions);
        json.GetProperty("isRevealed").GetBoolean().Should().BeFalse();
        json.GetProperty("myAnswer").ValueKind.Should().NotBe(JsonValueKind.Null);
        json.GetProperty("partnerAnswer").ValueKind.Should().Be(JsonValueKind.Null);
    }

    [Fact]
    public async Task GetAnswers_BothAnswered_IsRevealedTrue()
    {
        var (questionId, alice, bob) = await SetupAsync("get-both");

        await alice.PostAsJsonAsync("/v1/answers", new { questionId, text = "Alice's answer" });
        await bob.PostAsJsonAsync("/v1/answers", new { questionId, text = "Bob's answer" });

        var response = await alice.GetAsync($"/v1/answers/{questionId}");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var json = JsonSerializer.Deserialize<JsonElement>(
            await response.Content.ReadAsStringAsync(), JsonOptions);
        json.GetProperty("isRevealed").GetBoolean().Should().BeTrue();
        json.GetProperty("myAnswer").ValueKind.Should().NotBe(JsonValueKind.Null);
        json.GetProperty("partnerAnswer").ValueKind.Should().NotBe(JsonValueKind.Null);
    }
}
