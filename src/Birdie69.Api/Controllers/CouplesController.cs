using Birdie69.Application.Features.Couples.Commands.CreateCouple;
using Birdie69.Application.Features.Couples.Commands.JoinCouple;
using Birdie69.Application.Features.Couples.Commands.LeaveCouple;
using Birdie69.Application.Features.Couples.Commands.SetNotificationTime;
using Birdie69.Application.Features.Couples.Queries.GetCouple;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Birdie69.Api.Controllers;

[Route("v1/couples")]
public sealed class CouplesController(ISender sender) : ApiControllerBase
{
    /// <summary>
    /// Generate a new invite code and create a pending couple.
    /// If a pending couple already exists, regenerates its code (invalidating the old one).
    /// Returns 409 if the caller is already in an active couple.
    /// </summary>
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Create(CancellationToken cancellationToken)
    {
        var result = await sender.Send(new CreateCoupleCommand(), cancellationToken);
        if (result.IsFailure)
            return ToErrorResult(result.Error);

        return Created(string.Empty, new { inviteCode = result.Value });
    }

    /// <summary>Get the authenticated user's current active couple.</summary>
    [HttpGet("me")]
    [ProducesResponseType<CoupleDto>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetMine(CancellationToken cancellationToken)
        => ToActionResult(await sender.Send(new GetCoupleQuery(), cancellationToken));

    /// <summary>Join a couple using an invite code.</summary>
    [HttpPost("join")]
    [ProducesResponseType<CoupleDto>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Join(
        [FromBody] JoinCoupleCommand command,
        CancellationToken cancellationToken)
    {
        var joinResult = await sender.Send(command, cancellationToken);
        if (joinResult.IsFailure)
            return ToErrorResult(joinResult.Error);

        // Fetch full CoupleDto after successful join
        var coupleResult = await sender.Send(new GetCoupleQuery(), cancellationToken);
        return ToActionResult(coupleResult);
    }

    /// <summary>
    /// Leave the current couple.
    /// Pending + initiator → Cancel. Active + member → Disband. Returns 204.
    /// </summary>
    [HttpDelete("me")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Leave(CancellationToken cancellationToken)
        => ToActionResult(await sender.Send(new LeaveCoupleCommand(), cancellationToken));

    /// <summary>Set the daily notification time for the caller's couple.</summary>
    [HttpPut("me/notification-time")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> SetNotificationTime(
        [FromBody] SetNotificationTimeRequest request,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(new SetNotificationTimeCommand(request.NotificationTime), cancellationToken);
        if (result.IsFailure)
            return ToErrorResult(result.Error);

        return Ok(new { notificationTime = result.Value });
    }
}

/// <summary>Request body for PUT /v1/couples/me/notification-time</summary>
public sealed record SetNotificationTimeRequest(string NotificationTime);
