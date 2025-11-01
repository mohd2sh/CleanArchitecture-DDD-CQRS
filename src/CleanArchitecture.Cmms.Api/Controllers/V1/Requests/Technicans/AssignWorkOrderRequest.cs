namespace CleanArchitecture.Cmms.Api.Controllers.V1.Requests.Technicans;

public sealed class AssignWorkOrderRequest
{
    public Guid WorkOrderId { get; init; }
    public DateTime AssignedOn { get; init; }
}
