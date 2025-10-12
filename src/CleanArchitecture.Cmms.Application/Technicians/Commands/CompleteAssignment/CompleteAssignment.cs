namespace CleanArchitecture.Cmms.Application.Technicians.Commands.CompleteAssignment
{
    public sealed record CompleteAssignmentCommand(Guid TechnicianId, Guid WorkOrderId, DateTime CompletedOn) : ICommand<Result>;

}
