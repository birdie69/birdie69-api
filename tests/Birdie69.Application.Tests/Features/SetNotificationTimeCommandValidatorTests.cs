using Birdie69.Application.Features.Couples.Commands.SetNotificationTime;

namespace Birdie69.Application.Tests.Features;

public sealed class SetNotificationTimeCommandValidatorTests
{
    private readonly SetNotificationTimeCommandValidator _validator = new();

    [Theory]
    [InlineData("08:00")]
    [InlineData("20:30")]
    [InlineData("00:00")]
    [InlineData("23:59")]
    public void Validate_ValidTime_NoValidationErrors(string time)
    {
        var result = _validator.Validate(new SetNotificationTimeCommand(time));
        result.IsValid.Should().BeTrue();
    }

    [Theory]
    [InlineData("25:00")]
    [InlineData("24:00")]
    [InlineData("08:60")]
    [InlineData("8:00")]
    [InlineData("abc")]
    [InlineData("")]
    public void Validate_InvalidTime_HasValidationError(string time)
    {
        var result = _validator.Validate(new SetNotificationTimeCommand(time));
        result.IsValid.Should().BeFalse();
    }
}
