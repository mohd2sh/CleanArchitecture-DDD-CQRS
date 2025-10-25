namespace CleanArchitecture.Cmms.Application.Abstractions.Common;

/// <summary>
/// Represents an application error with a code, message, and type.
/// </summary>
public sealed class Error
{
    public string Code { get; }
    public string Message { get; }
    public ErrorType Type { get; }

    private Error(string code, string message, ErrorType type)
    {
        Code = code;
        Message = message;
        Type = type;
    }

    public static Error Validation(string code, string message)
        => new(code, message, ErrorType.Validation);

    public static Error NotFound(string code, string message)
        => new(code, message, ErrorType.NotFound);

    public static Error Conflict(string code, string message)
        => new(code, message, ErrorType.Conflict);

    public static Error Unauthorized(string code, string message)
        => new(code, message, ErrorType.Unauthorized);

    public static Error Forbidden(string code, string message)
        => new(code, message, ErrorType.Forbidden);

    public static Error Failure(string code, string message)
        => new(code, message, ErrorType.Failure);

    public static Error Failure(string message)
        => new("General.Failure", message, ErrorType.Failure);

    public static implicit operator Error(string message) => Failure(message);
}
