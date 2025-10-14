using CleanArchitecture.Cmms.Domain.Abstractions;

namespace CleanArchitecture.Cmms.Domain.WorkOrders.Events
{
    public sealed class WorkOrderCreatedEvent : IDomainEvent
    {
        public Guid WorkOrderId { get; }
        public string Title { get; }
        public Guid AssetId { get; }
        public DateTime? OccurredOn { get; } = DateTime.UtcNow;
        public WorkOrderCreatedEvent(Guid id, Guid assetId, string title)
        {
            WorkOrderId = id;
            AssetId = assetId;
            Title = title;
        }
    }
}
