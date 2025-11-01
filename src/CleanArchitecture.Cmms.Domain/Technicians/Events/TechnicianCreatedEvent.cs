using CleanArchitecture.Core.Domain.Abstractions;

namespace CleanArchitecture.Cmms.Domain.Technicians.Events;

public sealed record TechnicianCreatedEvent(Guid TechnicianId, string Name, string SkillLevelName, DateTime? OccurredOn = null) : IDomainEvent;
