namespace CleanArchitecture.Cmms.Application.WorkOrders.Commands.CompleteWorkOrder
{
    public sealed record CompleteWorkOrderCommand(Guid WorkOrderId) : ICommand<Result>;
}
