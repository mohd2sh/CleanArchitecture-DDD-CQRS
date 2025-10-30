using CleanArchitecture.Core.Domain.Abstractions;

namespace CleanArchitecture.Cmms.Domain.Technicians.Events
{
    public sealed record TechnicianUnAssignedToWorkOrderEvent(Guid TechnicianId, Guid WorkOrderId, DateTime? OccurredOn = null) : IDomainEvent;

}
