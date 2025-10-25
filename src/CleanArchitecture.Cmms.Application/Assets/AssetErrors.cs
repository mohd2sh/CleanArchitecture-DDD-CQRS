namespace CleanArchitecture.Cmms.Application.Assets;

using CleanArchitecture.Cmms.Application.Abstractions.Common;
using CleanArchitecture.Cmms.Domain.Abstractions.Attributes;

/// <summary>
/// Provides centralized error definitions for Asset operations.
/// </summary>
[ErrorCodeDefinition("Asset")]
public static class AssetErrors
{
    [ApplicationError]
    public static readonly Error NotFound = Error.NotFound(
        "Asset.NotFound",
        "Asset not found.");

    [ApplicationError]
    public static readonly Error NotAvailable = Error.Validation(
        "Asset.NotAvailable",
        "Asset is not available for work order.");

    [ApplicationError]
    public static readonly Error AlreadyUnderMaintenance = Error.Conflict(
        "Asset.AlreadyUnderMaintenance",
        Domain.Assets.AssetErrors.AlreadyUnderMaintenance.Message);

    [ApplicationError]
    public static readonly Error NotUnderMaintenance = Error.Validation(
        "Asset.NotUnderMaintenance",
        Domain.Assets.AssetErrors.NotUnderMaintenance.Message);

    [ApplicationError]
    public static readonly Error ConcurrencyConflict = Error.Conflict(
        "Asset.ConcurrencyConflict",
        "Asset was modified by another user. Please refresh and try again.");
}
