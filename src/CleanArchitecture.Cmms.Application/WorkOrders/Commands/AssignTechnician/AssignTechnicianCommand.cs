using CleanArchitecture.Cmms.Application.Abstractions.Messaging;
using CleanArchitecture.Cmms.Application.Primitives;

namespace CleanArchitecture.Cmms.Application.WorkOrders.Commands.AssignTechnician
{
    public sealed record AssignTechnicianCommand(Guid WorkOrderId, Guid TechnicianId)
    : ICommand<Result>;
}
