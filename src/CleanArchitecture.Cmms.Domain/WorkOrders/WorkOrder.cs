using CleanArchitecture.Cmms.Domain.Abstractions;
using CleanArchitecture.Cmms.Domain.WorkOrders.Enitties;
using CleanArchitecture.Cmms.Domain.WorkOrders.Enums;
using CleanArchitecture.Cmms.Domain.WorkOrders.Events;
using CleanArchitecture.Cmms.Domain.WorkOrders.ValueObjects;

namespace CleanArchitecture.Cmms.Domain.WorkOrders;

internal sealed class WorkOrder : AggregateRoot<Guid>
{
    private readonly List<TaskStep> _steps = new();
    private readonly List<Comment> _comments = new();

    private WorkOrder() { }

    private WorkOrder(Guid id, Guid assetId, string title, Location location, WorkOrderStatus status) : base(id)
    {
        AssetId = assetId;
        Title = title;
        Location = location;
        Status = status;
        Raise(new WorkOrderCreatedEvent(Id, AssetId, title));
    }

    public string Title { get; private set; } = default!;
    public Location Location { get; private set; }
    public WorkOrderStatus Status { get; private set; }
    public Guid? TechnicianId { get; private set; }
    public Guid AssetId { get; private set; }

    public IReadOnlyCollection<TaskStep> Steps => _steps.AsReadOnly();
    public IReadOnlyCollection<Comment> Comments => _comments.AsReadOnly();

    public static WorkOrder Create(Guid assetId, string title, Location location)
    {
        if (string.IsNullOrWhiteSpace(title))
            throw new DomainException("Work order title cannot be empty.");

        if (assetId == Guid.Empty)
            throw new DomainException("Asset ID must be specified.");

        return new(Guid.NewGuid(), assetId, title, location, WorkOrderStatus.Open);
    }

    internal void AssignTechnician(Guid technicianId)
    {
        if (Status is WorkOrderStatus.Cancelled or WorkOrderStatus.Completed)
            throw new DomainException("Invalid state");

        TechnicianId = technicianId;

        Status = Status == WorkOrderStatus.Open ? WorkOrderStatus.Assigned : Status;

        Raise(new TechnicianAssignedEvent(Id, technicianId));
    }

    internal void AddStep(string description)
    {
        if (string.IsNullOrWhiteSpace(description))
            throw new DomainException("Description required");

        _steps.Add(new TaskStep(description));
    }
    internal void AddComment(string text, Guid authorId)
    {
        if (string.IsNullOrWhiteSpace(text)) throw new DomainException("Text required");
        _comments.Add(new Comment(text, authorId));
    }

    internal void Start()
    {
        if (Status != WorkOrderStatus.Assigned)
            throw new DomainException("Invalid state");

        Status = WorkOrderStatus.InProgress;
    }

    internal void Complete()
    {
        if (Status != WorkOrderStatus.InProgress)
            throw new DomainException("Invalid state");

        if (Steps.Count > 0 && Steps.Any(s => !s.Completed))
            throw new DomainException("All steps must be completed");

        Status = WorkOrderStatus.Completed;

        Raise(new WorkOrderCompletedEvent(Id, AssetId, TechnicianId.GetValueOrDefault()));
    }

    internal void Cancel()
    {
        if (Status == WorkOrderStatus.Completed)
            throw new DomainException("Invalid state");

        Status = WorkOrderStatus.Cancelled;
    }
}
