using Birdie69.Application.Features.Users.Queries.GetProfile;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Birdie69.Api.Controllers;

[Route("v1/profile")]
public sealed class ProfileController(ISender sender) : ApiControllerBase
{
    /// <summary>Get own profile.</summary>
    [HttpGet]
    [ProducesResponseType<UserProfileDto>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Get(CancellationToken cancellationToken)
        => ToActionResult(await sender.Send(new GetProfileQuery(), cancellationToken));
}
