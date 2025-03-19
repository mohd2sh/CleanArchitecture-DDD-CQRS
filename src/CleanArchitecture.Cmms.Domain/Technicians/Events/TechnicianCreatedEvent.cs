using CleanArchitecture.Cmms.Domain.Abstractions;
using CleanArchitecture.Cmms.Domain.Technicians.ValueObjects;

namespace CleanArchitecture.Cmms.Domain.Technicians.Events
{
    public sealed record TechnicianCreatedEvent(Guid TechnicianId, string Name, SkillLevel SkillLevel, DateTime? OccurredOn = null) : IDomainEvent;
}
