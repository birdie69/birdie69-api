namespace Birdie69.Application.Features.Users.Queries.GetProfile;

public sealed record UserProfileDto(
    Guid Id,
    string DisplayName,
    string? AvatarUrl);
