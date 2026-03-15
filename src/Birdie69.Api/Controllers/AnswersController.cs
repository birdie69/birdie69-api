using Birdie69.Application.Features.Answers.Commands.SubmitAnswer;
using Birdie69.Application.Features.Answers.Queries.GetAnswers;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Birdie69.Api.Controllers;

public sealed class AnswersController(ISender sender) : ApiControllerBase
{
    /// <summary>Submit an answer to a question.</summary>
    [HttpPost]
    [ProducesResponseType<Guid>(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Submit(
        [FromBody] SubmitAnswerCommand command,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(command, cancellationToken);
        return result.IsSuccess
            ? CreatedAtAction(nameof(GetAnswers), new { questionId = command.QuestionId }, new { id = result.Value })
            : ToErrorResult(result.Error);
    }

    /// <summary>
    /// Get the reveal state for both partners' answers on a question.
    /// Always returns 200 — IsRevealed is true only when both have submitted.
    /// </summary>
    [HttpGet("{questionId:guid}")]
    [ProducesResponseType<AnswerRevealDto>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetAnswers(
        Guid questionId,
        CancellationToken cancellationToken)
        => ToActionResult(await sender.Send(new GetAnswersQuery(questionId), cancellationToken));
}
