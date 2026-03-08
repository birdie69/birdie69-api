using Birdie69.Domain.ValueObjects;
using FluentAssertions;

namespace Birdie69.Domain.Tests.ValueObjects;

public sealed class InviteCodeTests
{
    [Fact]
    public void Generate_ShouldReturn8CharCode()
    {
        var code = InviteCode.Generate();
        code.Value.Should().HaveLength(8);
    }

    [Fact]
    public void From_WithValidCode_ShouldSucceed()
    {
        var result = InviteCode.From("ABCD1234");
        result.IsSuccess.Should().BeTrue();
        result.Value.Value.Should().Be("ABCD1234");
    }

    [Fact]
    public void From_WithShortCode_ShouldFail()
    {
        var result = InviteCode.From("ABC");
        result.IsFailure.Should().BeTrue();
    }

    [Fact]
    public void TwoCodes_WithSameValue_ShouldBeEqual()
    {
        var a = InviteCode.From("ABCD1234").Value;
        var b = InviteCode.From("ABCD1234").Value;
        a.Should().Be(b);
    }
}
