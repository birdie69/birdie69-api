using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Security.Claims;
using Microsoft.IdentityModel.Tokens;
using System.Text;

namespace Birdie69.Integration.Tests;

public sealed class UsersEndpointTests(WebAppFactory factory)
    : IClassFixture<WebAppFactory>
{
    private HttpClient AuthClient()
    {
        var client = factory.CreateClient();
        // Any non-JWT value is replaced by a dev self-signed token whose sub equals the bearer value itself
        client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", "dev-test-token");
        return client;
    }

    [Fact]
    public async Task PutMe_WithDevToken_Returns200AndUserId()
    {
        var client = AuthClient();

        var response = await client.PutAsJsonAsync("/v1/users/me", new
        {
            displayName = "Integration Test User",
            avatarUrl = (string?)null
        });

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await response.Content.ReadAsStringAsync();
        body.Should().Contain("id");
    }

    [Fact]
    public async Task GetMe_AfterPut_ReturnsProfileWithCorrectDisplayName()
    {
        var client = AuthClient();

        // Create / update user
        await client.PutAsJsonAsync("/v1/users/me", new
        {
            displayName = "Alice Wonderland",
            avatarUrl = (string?)null
        });

        // Retrieve profile
        var response = await client.GetAsync("/v1/users/me");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await response.Content.ReadAsStringAsync();
        body.Should().Contain("Alice Wonderland");
    }

    [Fact]
    public async Task GetMe_WithoutPut_Returns404()
    {
        // Generate a real JWT with a unique sub that has never been PUT to the DB.
        // A real JWT is NOT replaced by the dev handler, so the unique sub is preserved.
        var client = factory.CreateClient();
        client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", CreateDevJwt("ext-never-registered-user"));

        var response = await client.GetAsync("/v1/users/me");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    /// <summary>
    /// Produces a signed JWT using the same dev key as Program.cs,
    /// so the JwtBearer handler accepts it without replacing it with the default dev token.
    /// </summary>
    private static string CreateDevJwt(string sub)
    {
        var key = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes("birdie69-dev-only-key-not-used-in-prod!"));
        var token = new JwtSecurityToken(
            claims: [new Claim(ClaimTypes.NameIdentifier, sub)],
            signingCredentials: new SigningCredentials(key, SecurityAlgorithms.HmacSha256));
        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    [Fact]
    public async Task PutMe_WithoutAuth_Returns401()
    {
        var client = factory.CreateClient();

        var response = await client.PutAsJsonAsync("/v1/users/me", new
        {
            displayName = "No Auth",
            avatarUrl = (string?)null
        });

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }
}
