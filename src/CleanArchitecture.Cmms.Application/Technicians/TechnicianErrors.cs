namespace CleanArchitecture.Cmms.Application.Technicians;

using CleanArchitecture.Cmms.Application.Primitives;
using CleanArchitecture.Cmms.Domain.Abstractions.Attributes;

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

    [ApplicationError]
    public static readonly Error Unavailable = Error.Validation(
        "Technician.Unavailable",
        Domain.Technicians.TechnicianErrors.Unavailable.Message);

    [ApplicationError]
    public static readonly Error AlreadyAssigned = Error.Conflict(
        "Technician.AlreadyAssigned",
        Domain.Technicians.TechnicianErrors.AlreadyAssigned.Message);

    [ApplicationError]
    public static readonly Error MaxAssignmentsReached = Error.Conflict(
        "Technician.MaxAssignmentsReached",
        Domain.Technicians.TechnicianErrors.MaxAssignmentsReached.Message);
}
