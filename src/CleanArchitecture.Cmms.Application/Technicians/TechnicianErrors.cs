namespace CleanArchitecture.Cmms.Application.Technicians;

using CleanArchitecture.Core.Application.Abstractions.Common;
using CleanArchitecture.Core.Domain.Abstractions.Attributes;

/// <summary>
/// Provides centralized error definitions for Technician operations.
/// </summary>
[ErrorCodeDefinition("Technician")]
public static class TechnicianErrors
{
    [ApplicationError]
    public static readonly Error NotFound = Error.NotFound(
        "Technician.NotFound",
        "Technician not found.");
}
