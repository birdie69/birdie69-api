namespace Birdie69.Domain.Common;

public enum ErrorType { Failure, NotFound, Validation, Conflict, Unauthorized }

/// <summary>
/// Represents a domain error with a code, description, and type.
/// Used with Result&lt;T&gt; to communicate failures without exceptions.
/// </summary>
public sealed record Error(string Code, string Description, ErrorType Type = ErrorType.Failure)
{
    public static readonly Error None = new(string.Empty, string.Empty);
    public static readonly Error NullValue = new("Error.NullValue", "A null value was provided.");

    public static Error NotFound(string entity, object id) =>
        new($"{entity}.NotFound", $"{entity} with id '{id}' was not found.", ErrorType.NotFound);

    public static Error Validation(string code, string description) =>
        new(code, description, ErrorType.Validation);

    public static Error Conflict(string code, string description) =>
        new(code, description, ErrorType.Conflict);

    public static Error Unauthorized(string code = "Error.Unauthorized", string description = "Unauthorized.") =>
        new(code, description, ErrorType.Unauthorized);
}
