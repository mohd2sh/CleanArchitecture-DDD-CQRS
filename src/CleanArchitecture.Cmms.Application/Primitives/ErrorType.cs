namespace CleanArchitecture.Cmms.Application.Primitives;

/// <summary>
/// Defines the type of error
/// </summary>
public enum ErrorType
{
    Validation,

    NotFound,

    Conflict,

    Unauthorized,

    Forbidden,

    Failure
}
