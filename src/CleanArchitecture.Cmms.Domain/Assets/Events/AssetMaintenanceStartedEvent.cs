using CleanArchitecture.Core.Domain.Abstractions  ;

namespace CleanArchitecture.Cmms.Domain.Assets.Events
{
    public sealed record AssetMaintenanceStartedEvent(
        Guid AssetId,
        Guid MaintenanceRecordId,
        DateTime StartedOn,
        string Description,
        string PerformedBy,
        DateTime? OccurredOn = null) : IDomainEvent;
}