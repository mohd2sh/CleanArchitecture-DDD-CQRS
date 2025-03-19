using CleanArchitecture.Cmms.Application.Abstractions.Messaging;
using CleanArchitecture.Cmms.Application.Primitives;

namespace CleanArchitecture.Cmms.Application.WorkOrders.Commands.CreateWorkOrder
{
    public sealed record CreateWorkOrderCommand(
        string Title,
        string Building,
        string Floor,
        string Room
     ) : ICommand<Result<Guid>>;
}
