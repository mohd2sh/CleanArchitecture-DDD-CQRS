using CleanArchitecture.Cmms.Domain.Abstractions;
using CleanArchitecture.Cmms.Domain.Assets.ValueObjects;

namespace CleanArchitecture.Cmms.Domain.Assets.Events
{
    public sealed record AssetLocationUpdatedEvent(
      Guid AssetId,
      AssetLocation NewLocation,
      DateTime? OccurredOn = null
  ) : IDomainEvent;
}
