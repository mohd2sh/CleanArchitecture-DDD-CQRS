using CleanArchitecture.Cmms.Domain.Abstractions;

namespace CleanArchitecture.Cmms.Domain.Assets.Events
{
    public sealed record AssetMaintenanceCompletedEvent(
        Guid AssetId,
        DateTime CompletedOn,
        string Notes,
        DateTime? OccurredOn = null
        ) : IDomainEvent;
}
