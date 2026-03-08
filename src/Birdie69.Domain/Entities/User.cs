using Birdie69.Domain.Common;
using Birdie69.Domain.Events;

namespace Birdie69.Domain.Entities;

/// <summary>
/// Represents an authenticated user. Identity is managed by Azure AD B2C;
/// ExternalId is the B2C Object ID used to correlate incoming tokens.
/// No passwords are stored here.
/// </summary>
public sealed class User : AuditableEntity
{
    public string ExternalId { get; private set; } = string.Empty;
    public string DisplayName { get; private set; } = string.Empty;
    public string? AvatarUrl { get; private set; }
    public string? NotificationToken { get; private set; }
    public bool IsActive { get; private set; } = true;

    private User() { }

    public static User Create(Guid id, string externalId, string displayName, string? avatarUrl = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(externalId);
        ArgumentException.ThrowIfNullOrWhiteSpace(displayName);

        var user = new User
        {
            Id = id,
            ExternalId = externalId,
            DisplayName = displayName,
            AvatarUrl = avatarUrl,
        };

        user.RaiseDomainEvent(new UserCreatedEvent(user.Id, externalId, displayName));
        return user;
    }

    public void UpdateProfile(string displayName, string? avatarUrl)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(displayName);
        DisplayName = displayName;
        AvatarUrl = avatarUrl;
    }

    public void SetNotificationToken(string? token)
    {
        NotificationToken = token;
    }

    public void Deactivate()
    {
        IsActive = false;
    }
}
