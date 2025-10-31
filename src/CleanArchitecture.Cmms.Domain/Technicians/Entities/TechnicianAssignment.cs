using CleanArchitecture.Core.Domain.Abstractions;

namespace CleanArchitecture.Cmms.Domain.Technicians.Entities;

internal sealed class TechnicianAssignment : Entity<Guid>
{
    public Guid WorkOrderId { get; private set; }
    public DateTime AssignedOn { get; private set; }
    public DateTime? CompletedOn { get; private set; }

    public bool IsCompleted => CompletedOn is not null;

    private TechnicianAssignment() { } // EF

    private TechnicianAssignment(Guid id, Guid workOrderId, DateTime assignedOn)
        : base(id)
    {
        WorkOrderId = workOrderId;
        AssignedOn = assignedOn;
    }

    public static TechnicianAssignment Create(Guid workOrderId, DateTime assignedOn)
        => new(Guid.NewGuid(), workOrderId, assignedOn);

    public void CompleteAssignment(DateTime completedOn)
    {
        if (IsCompleted)
            return;

        CompletedOn = completedOn;
    }
}
