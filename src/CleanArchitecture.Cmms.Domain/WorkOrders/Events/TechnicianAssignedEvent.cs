using CleanArchitecture.Cmms.Domain.Abstractions;

namespace CleanArchitecture.Cmms.Domain.WorkOrders.Events
{
    public sealed class TechnicianAssignedEvent : IDomainEvent
    {
        public Guid WorkOrderId { get; }
        public Guid TechnicianId { get; }
        public DateTime? OccurredOn { get; } = DateTime.UtcNow;
        public TechnicianAssignedEvent(Guid workOrderId, Guid technicianId)
        {
            WorkOrderId = workOrderId;
            TechnicianId = technicianId;
        }
    }
}
