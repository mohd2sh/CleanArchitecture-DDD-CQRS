using CleanArchitecture.Cmms.Domain.Abstractions;
using CleanArchitecture.Cmms.Domain.Abstractions.Attributes;


namespace CleanArchitecture.Cmms.Domain.Assets;

/// <summary>
/// Provides error messages for Asset domain invariants.
/// </summary>
[ErrorCodeDefinition("Asset")]
public static class AssetErrors
{
    [DomainError]
    public static readonly DomainError AlreadyUnderMaintenance = DomainError.Create(
        "Asset.AlreadyUnderMaintenance",
        "Asset already under maintenance.");

    [DomainError]
    public static readonly DomainError NotUnderMaintenance = DomainError.Create(
        "Asset.NotUnderMaintenance",
        "Asset is not under maintenance.");


    [DomainError]
    public static readonly DomainError MaintenanceDescriptionRequired = DomainError.Create(
        "AssetMaintenanceRecord.MaintenanceDescriptionRequired",
        "Description is required when creating Maintenance record.");


    [DomainError]
    public static readonly DomainError MaintenancePerformerRequired = DomainError.Create(
        "AssetMaintenanceRecord.MaintenancePerformerRequired",
        "PerformedBy is required when creating Maintenance record.");


    [DomainError]
    public static readonly DomainError TagRequired = DomainError.Create(
       "AssetTag.TagRequired",
       "Asset tag cannot be empty.");
}
