using CleanArchitecture.Core.Application.Abstractions.Common;

namespace CleanArchitecture.Cmms.Application.WorkOrders.Commands.StartWorkOrder;

public sealed record StartWorkOrderCommand(Guid WorkOrderId) : ICommand<Result>;
