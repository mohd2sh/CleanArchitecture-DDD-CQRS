using CleanArchitecture.Cmms.Domain.Abstractions;
using CleanArchitecture.Cmms.Domain.Abstractions.Attributes;

namespace CleanArchitecture.Cmms.Domain.Technicians;

/// <summary>
/// Provides error messages for Technician domain invariants.
/// </summary>
[ErrorCodeDefinition("Technician")]
public static class TechnicianErrors
{
    [DomainError]
    public static readonly DomainError Unavailable = DomainError.Create(
        "Technician.Unavailable",
        "Technician is unavailable.");

    [DomainError]
    public static readonly DomainError AlreadyAssigned = DomainError.Create(
        "Technician.AlreadyAssigned",
        "Technician already assigned to this active work order.");

    [DomainError]
    public static readonly DomainError MaxAssignmentsReached = DomainError.Create(
        "Technician.MaxAssignmentsReached",
        "Technician reached maximum concurrent work orders.");

    [DomainError]
    public static readonly DomainError CertificationExists = DomainError.Create(
        "Technician.CertificationExists",
        "Technician already has this certification.");

    [DomainError]
    public static readonly DomainError AssignmentNotFound = DomainError.Create(
        "Technician.AssignmentNotFound",
        "Assignment not found.");

    [DomainError]
    public static readonly DomainError AlreadyUnderMaintenance = DomainError.Create(
        "Asset.AlreadyUnderMaintenance",
        "Asset is already under maintenance.");

    [DomainError]
    public static readonly DomainError NotUnderMaintenance = DomainError.Create(
        "Asset.NotUnderMaintenance",
        "Asset is not under maintenance.");

    [DomainError]
    public static readonly DomainError MaintenanceDescriptionRequired = DomainError.Create(
        "Asset.MaintenanceDescriptionRequired",
        "Maintenance description cannot be empty.");

    [DomainError]
    public static readonly DomainError MaintenancePerformerRequired = DomainError.Create(
        "Asset.MaintenancePerformerRequired",
        "Maintenance performer name cannot be empty.");

    [DomainError]
    public static readonly DomainError LevelNameRequired = DomainError.Create(
    "SkillLevel.LevelNameRequired",
    "Level name cannot be empty");

    [DomainError]
    public static readonly DomainError InvalidRank = DomainError.Create(
        "SkillLevel.InvalidRank",
        "Rank must be positive");
}
