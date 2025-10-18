namespace CleanArchitecture.Cmms.Domain.Abstractions.Attributes;

/// <summary>
/// Marks a DomainError field for export and future localization.
/// Error code is extracted from the DomainError object itself.
/// </summary>
[AttributeUsage(AttributeTargets.Field, AllowMultiple = false, Inherited = false)]
public sealed class DomainErrorAttribute : Attribute
{
    // Simple marker attribute - code comes from DomainError object
}
