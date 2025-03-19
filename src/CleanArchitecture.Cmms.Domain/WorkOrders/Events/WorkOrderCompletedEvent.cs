using CleanArchitecture.Cmms.Domain.Abstractions;

namespace CleanArchitecture.Cmms.Domain.WorkOrders.Events;

public sealed class WorkOrderCompletedEvent : IDomainEvent
{
    public Guid WorkOrderId { get; }
    public DateTime? OccurredOn { get; } = DateTime.UtcNow;
    public WorkOrderCompletedEvent(Guid id) => WorkOrderId = id;
}
