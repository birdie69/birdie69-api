using Birdie69.Domain.Common;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Birdie69.Api.Controllers;

[ApiController]
[Authorize]
[Route("v1/[controller]")]
public abstract class ApiControllerBase : ControllerBase
{
    protected IActionResult ToActionResult<T>(Result<T> result) => result.IsSuccess
        ? Ok(result.Value)
        : result.Error.Code.EndsWith(".NotFound") ? NotFound(result.Error)
        : result.Error.Code.StartsWith("Error.Unauthorized") ? Forbid()
        : UnprocessableEntity(result.Error);
}
