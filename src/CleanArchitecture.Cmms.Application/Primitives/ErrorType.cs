namespace CleanArchitecture.Cmms.Application.Primitives;

/// <summary>
/// Defines the type of error
/// </summary>
public enum ErrorType
{
    /// <summary>400 Bad Request - Validation errors</summary>
    Validation,

    /// <summary>404 Not Found - Resource not found</summary>
    NotFound,

    /// <summary>409 Conflict - Resource conflict</summary>
    Conflict,

    /// <summary>401 Unauthorized - Authentication required</summary>
    Unauthorized,

    /// <summary>403 Forbidden - Authorization failed</summary>
    Forbidden,

    /// <summary>400 Bad Request - General failure (default)</summary>
    Failure
}
