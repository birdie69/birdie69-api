using Birdie69.Application.Features.Users.Commands.SetNotificationToken;

namespace Birdie69.Application.Tests.Features;

public sealed class SetNotificationTokenCommandValidatorTests
{
    private readonly SetNotificationTokenCommandValidator _validator = new();

    [Fact]
    public void Validate_ValidToken_NoValidationErrors()
    {
        var result = _validator.Validate(new SetNotificationTokenCommand("fake-fcm-token-abc123"));
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void Validate_EmptyToken_HasValidationError()
    {
        var result = _validator.Validate(new SetNotificationTokenCommand(string.Empty));
        result.IsValid.Should().BeFalse();
    }

    [Fact]
    public void Validate_TokenExceeding512Chars_HasValidationError()
    {
        var longToken = new string('x', 513);
        var result = _validator.Validate(new SetNotificationTokenCommand(longToken));
        result.IsValid.Should().BeFalse();
    }

    [Fact]
    public void Validate_TokenExactly512Chars_NoValidationErrors()
    {
        var maxToken = new string('x', 512);
        var result = _validator.Validate(new SetNotificationTokenCommand(maxToken));
        result.IsValid.Should().BeTrue();
    }
}
