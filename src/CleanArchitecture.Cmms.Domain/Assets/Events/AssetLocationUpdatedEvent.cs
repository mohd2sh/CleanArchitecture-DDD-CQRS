using CleanArchitecture.Cmms.Domain.Abstractions;

namespace CleanArchitecture.Cmms.Domain.Assets.Events
{
    public sealed record AssetLocationUpdatedEvent(
      Guid AssetId,
      DateTime? OccurredOn = null
  ) : IDomainEvent;
}
