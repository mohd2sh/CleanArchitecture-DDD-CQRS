namespace CleanArchitecture.Cmms.Domain.Abstractions.Attributes;

/// <summary>
/// Marks a static readonly Error field as an application error.
/// Used for export and frontend localization.
/// </summary>
[AttributeUsage(AttributeTargets.Field, AllowMultiple = false, Inherited = false)]
public sealed class ApplicationErrorAttribute : Attribute
{
    // Error code is extracted from the Error object itself
}
