using CleanArchitecture.Cmms.Domain.Abstractions;
using CleanArchitecture.Cmms.Domain.Assets.Enums;

namespace CleanArchitecture.Cmms.Domain.Assets.Events
{

    public sealed record AssetStatusChangedEvent(
        Guid AssetId,
        AssetStatus NewStatus,
        DateTime? OccurredOn = null
    ) : IDomainEvent;
}
