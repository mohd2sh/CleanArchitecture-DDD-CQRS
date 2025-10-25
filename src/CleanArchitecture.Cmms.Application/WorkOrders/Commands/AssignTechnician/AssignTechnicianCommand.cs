using CleanArchitecture.Cmms.Application.Abstractions.Common;

namespace CleanArchitecture.Cmms.Application.WorkOrders.Commands.AssignTechnician
{
    public sealed record AssignTechnicianCommand(Guid WorkOrderId, Guid TechnicianId)
    : ICommand<Result>;
}
