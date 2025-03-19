using CleanArchitecture.Cmms.Domain.Abstractions;

namespace CleanArchitecture.Cmms.Domain.WorkOrders.Events
{
    public sealed class WorkOrderCreatedEvent : IDomainEvent
    {
        public Guid WorkOrderId { get; }
        public DateTime? OccurredOn { get; } = DateTime.UtcNow;
        public WorkOrderCreatedEvent(Guid id) => WorkOrderId = id;
    }
}
