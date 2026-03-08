using FluentValidation;

namespace Birdie69.Application.Features.Answers.Commands.SubmitAnswer;

public sealed class SubmitAnswerCommandValidator : AbstractValidator<SubmitAnswerCommand>
{
    public SubmitAnswerCommandValidator()
    {
        RuleFor(x => x.QuestionId).NotEmpty();
        RuleFor(x => x.Text)
            .NotEmpty()
            .MaximumLength(1000);
    }
}
