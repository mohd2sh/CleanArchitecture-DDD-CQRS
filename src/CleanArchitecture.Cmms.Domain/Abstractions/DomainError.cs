namespace CleanArchitecture.Cmms.Domain.Abstractions;

/// <summary>
/// Represents a domain error with a code and message.
/// Used to enforce centralized error management in the domain layer.
/// </summary>
public sealed class DomainError
{
    public string Code { get; }
    public string Message { get; }

    private DomainError(string code, string message)
    {
        Code = code;
        Message = message;
    }

    public static DomainError Create(string code, string message)
        => new(code, message);
}
