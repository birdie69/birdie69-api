using Birdie69.Domain.Common;
using MediatR;

namespace Birdie69.Application.Features.Users.Commands.UpsertUser;

/// <summary>
/// Called on first authenticated API call to ensure a local User record exists
/// that matches the Azure AD B2C identity.
/// </summary>
public sealed record UpsertUserCommand(
    string ExternalId,
    string DisplayName,
    string? AvatarUrl) : IRequest<Result<Guid>>;
