using CleanArchitecture.Cmms.Application.Abstractions.Messaging;
using CleanArchitecture.Cmms.Application.Primitives;

namespace CleanArchitecture.Cmms.Application.WorkOrders.Commands.AddStep
{
    public sealed record AddStepCommand(Guid WorkOrderId, string Title) : ICommand<Result>;

}
