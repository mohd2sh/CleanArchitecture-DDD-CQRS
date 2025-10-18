using CleanArchitecture.Cmms.Domain.Abstractions;
using CleanArchitecture.Cmms.Domain.Technicians.Entities;
using CleanArchitecture.Cmms.Domain.Technicians.Enums;
using CleanArchitecture.Cmms.Domain.Technicians.Events;
using CleanArchitecture.Cmms.Domain.Technicians.ValueObjects;

namespace CleanArchitecture.Cmms.Domain.Technicians
{
    internal sealed class Technician : AggregateRoot<Guid>
    {
        private readonly List<TechnicianAssignment> _assignments = new();

        private readonly List<Certification> _certifications = new();

        public string Name { get; private set; }
        public SkillLevel SkillLevel { get; private set; }
        public TechnicianStatus Status { get; private set; }
        public IReadOnlyCollection<Certification> Certifications => _certifications.AsReadOnly();
        public int MaxConcurrentAssignments { get; private set; } = 3;
        public IReadOnlyCollection<TechnicianAssignment> Assignments => _assignments.AsReadOnly();

        private Technician() { } // EF

        private Technician(Guid id, string name, SkillLevel skillLevel)
            : base(id)
        {
            Name = name;
            SkillLevel = skillLevel;
            Status = TechnicianStatus.Available;
        }

        public static Technician Create(string name, SkillLevel skillLevel)
        {
            var technician = new Technician(Guid.NewGuid(), name, skillLevel);

            technician.Raise(new TechnicianCreatedEvent(technician.Id, name, skillLevel.LevelName));

            return technician;
        }

        public void AddCertification(Certification certification)
        {
            if (_certifications.Any(c => c.Equals(certification)))
                throw new DomainException(TechnicianErrors.CertificationExists);

            _certifications.Add(certification);
            Raise(new TechnicianCertificationAddedEvent(Id, certification.Code));
        }

        public void AddAssignedOrder(Guid workOrderId, DateTime assignedOn)
        {
            if (Status == TechnicianStatus.Unavailable)
                throw new DomainException(TechnicianErrors.Unavailable);

            if (_assignments.Any(a => a.WorkOrderId == workOrderId && !a.IsCompleted))
                throw new DomainException(TechnicianErrors.AlreadyAssigned);

            if (_assignments.Count(a => !a.IsCompleted) >= MaxConcurrentAssignments)
                throw new DomainException(TechnicianErrors.MaxAssignmentsReached);

            var assignment = TechnicianAssignment.Create(workOrderId, assignedOn);
            _assignments.Add(assignment);

            Raise(new TechnicianAssignedToWorkOrderEvent(Id, workOrderId));
        }

        public void CompleteAssignment(Guid workOrderId, DateTime completedOn)
        {
            var assignment = _assignments.FirstOrDefault(a => a.WorkOrderId == workOrderId);
            if (assignment is null)
                throw new DomainException(TechnicianErrors.AssignmentNotFound);

            assignment.CompleteAssignment(completedOn);

            Raise(new TechnicianUnassignedFromWorkOrderEvent(Id, workOrderId));
        }

        public void SetUnavailable()
        {
            if (Status == TechnicianStatus.Unavailable)
                return;

            Status = TechnicianStatus.Unavailable;
            Raise(new TechnicianStatusChangedEvent(Id, Status));
        }

        public void SetAvailable()
        {
            if (Status == TechnicianStatus.Available)
                return;

            Status = TechnicianStatus.Available;
            Raise(new TechnicianStatusChangedEvent(Id, Status));
        }

        public bool IsAvailable() => Status == TechnicianStatus.Available;
    }
}
