using CleanArchitecture.Cmms.Domain.Abstractions;

namespace CleanArchitecture.Cmms.Domain.WorkOrders.Events;

public sealed class WorkOrderCompletedEvent : IDomainEvent
{
    public Guid WorkOrderId { get; }
    public Guid AssetId { get; }
    public Guid TechnicianId { get; }
    public DateTime? OccurredOn { get; } = DateTime.UtcNow;
    public WorkOrderCompletedEvent(Guid id, Guid assetId, Guid technicianId)
    {
        WorkOrderId = id;
        AssetId = assetId;
        TechnicianId = technicianId;
    }
}
