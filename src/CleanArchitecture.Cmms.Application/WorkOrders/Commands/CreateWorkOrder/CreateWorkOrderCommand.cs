using CleanArchitecture.Core.Application.Abstractions.Common;

namespace CleanArchitecture.Cmms.Application.WorkOrders.Commands.CreateWorkOrder;

public sealed record CreateWorkOrderCommand(
    Guid AssetId,
    string Title,
    string Building,
    string Floor,
    string Room
 ) : ICommand<Result<Guid>>;
