using CleanArchitecture.Cmms.Application.Abstractions.Messaging;
using CleanArchitecture.Cmms.Application.Primitives;

namespace CleanArchitecture.Cmms.Application.WorkOrders.Commands.CompleteWorkOrder
{
    public sealed record CompleteWorkOrderCommand(Guid WorkOrderId) : ICommand<Result>;
}
