using CleanArchitecture.Cmms.Domain.Technicians.Enums;
using CleanArchitecture.Core.Domain.Abstractions;

namespace CleanArchitecture.Cmms.Domain.Technicians.Events;

public sealed record TechnicianStatusChangedEvent(Guid TechnicianId, TechnicianStatus NewStatus, DateTime? OccurredOn = null) : IDomainEvent;
