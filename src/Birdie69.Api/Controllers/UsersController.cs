using Birdie69.Application.Common.Interfaces;
using Birdie69.Application.Features.Users.Commands.SetNotificationToken;
using Birdie69.Application.Features.Users.Commands.UpsertUser;
using Birdie69.Application.Features.Users.Queries.GetProfile;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Birdie69.Api.Controllers;

/// <summary>Request body for PUT /v1/users/me</summary>
public sealed record UpsertUserRequest(string DisplayName, string? AvatarUrl);

[Route("v1/users")]
public sealed class UsersController(ISender sender, ICurrentUser currentUser) : ApiControllerBase
{
    /// <summary>Get the authenticated user's profile.</summary>
    [HttpGet("me")]
    [ProducesResponseType<UserProfileDto>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetMe(CancellationToken cancellationToken)
        => ToActionResult(await sender.Send(new GetProfileQuery(), cancellationToken));

    /// <summary>
    /// Create or update the authenticated user's profile.
    /// Called on first sign-in to ensure a local record exists for the B2C identity.
    /// </summary>
    [HttpPut("me")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> UpsertMe(
        [FromBody] UpsertUserRequest request,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(
            new UpsertUserCommand(currentUser.ExternalId, request.DisplayName, request.AvatarUrl),
            cancellationToken);

        if (result.IsFailure)
            return ToErrorResult(result.Error);

        return Ok(new { id = result.Value });
    }

    /// <summary>Register or update the caller's FCM device token.</summary>
    [HttpPut("me/notification-token")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> SetNotificationToken(
        [FromBody] SetNotificationTokenRequest request,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(new SetNotificationTokenCommand(request.Token), cancellationToken);
        if (result.IsFailure)
            return ToErrorResult(result.Error);

        return Ok();
    }
}

/// <summary>Request body for PUT /v1/users/me/notification-token</summary>
public sealed record SetNotificationTokenRequest(string Token);
