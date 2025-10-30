using CleanArchitecture.Cmms.Domain.Assets.Enums;
using CleanArchitecture.Core.Domain.Abstractions;

namespace CleanArchitecture.Cmms.Domain.Assets.Events
{

    public sealed record AssetStatusChangedEvent(
        Guid AssetId,
        AssetStatus NewStatus,
        DateTime? OccurredOn = null
    ) : IDomainEvent;
}
