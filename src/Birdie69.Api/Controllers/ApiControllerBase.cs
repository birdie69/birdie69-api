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
        : ToErrorResult(result.Error);

    protected IActionResult ToActionResult(Result result) => result.IsSuccess
        ? NoContent()
        : ToErrorResult(result.Error);

    protected IActionResult ToErrorResult(Error error) => error.Type switch
    {
        ErrorType.NotFound    => NotFound(error),
        ErrorType.Conflict    => Conflict(error),
        ErrorType.Validation  => UnprocessableEntity(error),
        ErrorType.Unauthorized => Forbid(),
        _                     => UnprocessableEntity(error)
    };
}
