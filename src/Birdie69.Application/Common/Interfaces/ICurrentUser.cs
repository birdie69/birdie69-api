namespace Birdie69.Application.Common.Interfaces;

/// <summary>
/// Abstracts the currently authenticated user extracted from the JWT.
/// Implemented in the API layer via HttpContext.
/// </summary>
public interface ICurrentUser
{
    /// <summary>Azure AD B2C Object ID from the token subject claim.</summary>
    string ExternalId { get; }

    bool IsAuthenticated { get; }
}
