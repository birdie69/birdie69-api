using FluentValidation;

namespace Birdie69.Application.Features.Couples.Commands.SetNotificationTime;

public sealed class SetNotificationTimeCommandValidator : AbstractValidator<SetNotificationTimeCommand>
{
    public SetNotificationTimeCommandValidator()
    {
        RuleFor(x => x.Time)
            .NotEmpty()
            .Matches(@"^([01]\d|2[0-3]):[0-5]\d$")
            .WithMessage("Time must be in HH:mm format (e.g. 08:00).");
    }
}
