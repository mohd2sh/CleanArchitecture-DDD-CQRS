namespace CleanArchitecture.Core.Domain.Abstractions.Attributes;

/// <summary>
/// Marks a class as a container for error code definitions.
/// Used for discoverability, export, and architecture testing.
/// </summary>
[AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
public sealed class ErrorCodeDefinitionAttribute : Attribute
{
    /// <summary>
    /// The domain/aggregate this error provider belongs to (e.g., "WorkOrder", "Technician").
    /// </summary>
    public string Domain { get; }

    public ErrorCodeDefinitionAttribute(string domain)
    {
        Domain = domain;
    }
}
