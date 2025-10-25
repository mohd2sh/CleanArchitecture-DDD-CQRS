using CleanArchitecture.Core.Domain.Abstractions  ;

namespace CleanArchitecture.Cmms.Domain.Assets.Events
{
    public sealed record AssetCreatedEvent(
     Guid AssetId,
     string Name,
     string Type,
     string TagValue,
     DateTime? OccurredOn = null) : IDomainEvent;
}
