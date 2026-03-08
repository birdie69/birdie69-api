using Birdie69.Domain.Common;

namespace Birdie69.Domain.ValueObjects;

/// <summary>
/// A short, human-readable invite code used to form a couple.
/// Format: 8 uppercase alphanumeric characters (e.g. "A3BX7K2Q").
/// </summary>
public sealed class InviteCode : ValueObject
{
    private const int Length = 8;
    private static readonly char[] Alphabet =
        "ABCDEFGHJKLMNPQRSTUVWXYZ23456789".ToCharArray();

    public string Value { get; }

    private InviteCode(string value) => Value = value;

    public static InviteCode Generate()
    {
        var chars = new char[Length];
        var random = Random.Shared;
        for (var i = 0; i < Length; i++)
            chars[i] = Alphabet[random.Next(Alphabet.Length)];
        return new InviteCode(new string(chars));
    }

    public static Result<InviteCode> From(string value)
    {
        if (string.IsNullOrWhiteSpace(value) || value.Length != Length)
            return Result.Failure<InviteCode>(
                Error.Validation("InviteCode.Invalid", $"Invite code must be {Length} characters."));

        return new InviteCode(value.ToUpperInvariant());
    }

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Value;
    }

    public override string ToString() => Value;
}
