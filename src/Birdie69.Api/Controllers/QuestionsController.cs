using Birdie69.Application.Features.Questions.Queries.GetTodayQuestion;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Birdie69.Api.Controllers;

public sealed class QuestionsController(ISender sender) : ApiControllerBase
{
    /// <summary>Returns today's question.</summary>
    [HttpGet("today")]
    [ProducesResponseType<QuestionDto>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetToday(CancellationToken cancellationToken)
        => ToActionResult(await sender.Send(new GetTodayQuestionQuery(), cancellationToken));
}
