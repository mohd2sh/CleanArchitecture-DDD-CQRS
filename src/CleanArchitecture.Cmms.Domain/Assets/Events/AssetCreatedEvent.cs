using CleanArchitecture.Cmms.Domain.Abstractions;
using CleanArchitecture.Cmms.Domain.Assets.ValueObjects;

namespace CleanArchitecture.Cmms.Domain.Assets.Events
{
    public sealed record AssetCreatedEvent(
     Guid AssetId,
     string Name,
     string Type,
     AssetTag Tag,
     AssetLocation Location,
     DateTime? OccurredOn = null) : IDomainEvent;
}
