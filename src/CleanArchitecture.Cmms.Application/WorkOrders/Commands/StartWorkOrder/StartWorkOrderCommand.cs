using CleanArchitecture.Cmms.Application.Abstractions.Messaging;
using CleanArchitecture.Cmms.Application.Primitives;

namespace CleanArchitecture.Cmms.Application.WorkOrders.Commands.StartWorkOrder
{
    public sealed record StartWorkOrderCommand(Guid WorkOrderId) : ICommand<Result>;

}
