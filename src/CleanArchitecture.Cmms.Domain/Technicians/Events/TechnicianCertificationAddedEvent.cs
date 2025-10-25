using CleanArchitecture.Core.Domain.Abstractions;

namespace CleanArchitecture.Cmms.Domain.Technicians.Events
{
    public sealed record TechnicianCertificationAddedEvent(Guid TechnicianId, string CertificationCode, DateTime? OccurredOn = null) : IDomainEvent;

}
