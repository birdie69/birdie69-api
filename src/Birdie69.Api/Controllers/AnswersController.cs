using Birdie69.Application.Features.Answers.Commands.SubmitAnswer;
using Birdie69.Application.Features.Answers.Queries.GetAnswers;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Birdie69.Api.Controllers;

public sealed class AnswersController(ISender sender) : ApiControllerBase
{
    /// <summary>Submit an answer to a question.</summary>
    [HttpPost]
    [ProducesResponseType<Guid>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> Submit(
        [FromBody] SubmitAnswerCommand command,
        CancellationToken cancellationToken)
        => ToActionResult(await sender.Send(command, cancellationToken));

    /// <summary>
    /// Get both partners' answers for a question.
    /// Only available after both have submitted.
    /// </summary>
    [HttpGet("{questionId:guid}")]
    [ProducesResponseType<IReadOnlyList<AnswerDto>>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> GetAnswers(
        Guid questionId,
        CancellationToken cancellationToken)
        => ToActionResult(await sender.Send(new GetAnswersQuery(questionId), cancellationToken));
}
