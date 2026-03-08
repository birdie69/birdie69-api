using Birdie69.Application.Features.Couples.Commands.CreateCouple;
using Birdie69.Application.Features.Couples.Commands.JoinCouple;
using Birdie69.Application.Features.Couples.Queries.GetCouple;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Birdie69.Api.Controllers;

[Route("v1/couple")]
public sealed class CoupleController(ISender sender) : ApiControllerBase
{
    /// <summary>Get current couple info.</summary>
    [HttpGet]
    [ProducesResponseType<CoupleDto>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Get(CancellationToken cancellationToken)
        => ToActionResult(await sender.Send(new GetCoupleQuery(), cancellationToken));

    /// <summary>Generate a new invite code and create a pending couple.</summary>
    [HttpPost("invite")]
    [ProducesResponseType<string>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> CreateInvite(CancellationToken cancellationToken)
        => ToActionResult(await sender.Send(new CreateCoupleCommand(), cancellationToken));

    /// <summary>Join an existing couple via invite code.</summary>
    [HttpPost("join")]
    [ProducesResponseType<Guid>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> Join(
        [FromBody] JoinCoupleCommand command,
        CancellationToken cancellationToken)
        => ToActionResult(await sender.Send(command, cancellationToken));
}
