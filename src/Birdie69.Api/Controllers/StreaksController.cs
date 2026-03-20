using Birdie69.Application.Features.Streaks.Queries.GetStreak;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Birdie69.Api.Controllers;

public sealed class StreaksController(ISender sender) : ApiControllerBase
{
    /// <summary>
    /// Get the current user's streak.
    /// Always returns 200 — new users with no answers get 0/0/null.
    /// </summary>
    [HttpGet("me")]
    [ProducesResponseType<StreakDto>(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetMyStreak(CancellationToken cancellationToken)
        => ToActionResult(await sender.Send(new GetStreakQuery(), cancellationToken));
}
