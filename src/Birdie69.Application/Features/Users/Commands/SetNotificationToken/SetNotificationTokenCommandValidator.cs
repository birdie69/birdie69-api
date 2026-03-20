using FluentValidation;

namespace Birdie69.Application.Features.Users.Commands.SetNotificationToken;

public sealed class SetNotificationTokenCommandValidator : AbstractValidator<SetNotificationTokenCommand>
{
    public SetNotificationTokenCommandValidator()
    {
        RuleFor(x => x.Token)
            .NotEmpty()
            .MaximumLength(512);
    }
}
