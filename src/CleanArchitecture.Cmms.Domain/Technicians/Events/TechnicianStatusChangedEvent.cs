using CleanArchitecture.Cmms.Domain.Abstractions;
using CleanArchitecture.Cmms.Domain.Technicians.Enums;

namespace CleanArchitecture.Cmms.Domain.Technicians.Events
{
    public sealed record TechnicianStatusChangedEvent(Guid TechnicianId, TechnicianStatus NewStatus, DateTime? OccurredOn = null) : IDomainEvent;

}
