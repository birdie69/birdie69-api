using Birdie69.Application.Common.Interfaces;
using System.Security.Claims;

namespace Birdie69.Api.Services;

/// <summary>
/// Extracts the authenticated user's B2C Object ID from the JWT subject claim.
/// </summary>
public sealed class CurrentUserService(IHttpContextAccessor httpContextAccessor) : ICurrentUser
{
    public string ExternalId =>
        httpContextAccessor.HttpContext?.User.FindFirstValue(ClaimTypes.NameIdentifier)
        ?? httpContextAccessor.HttpContext?.User.FindFirstValue("sub")
        ?? string.Empty;

    public bool IsAuthenticated =>
        httpContextAccessor.HttpContext?.User.Identity?.IsAuthenticated ?? false;
}
