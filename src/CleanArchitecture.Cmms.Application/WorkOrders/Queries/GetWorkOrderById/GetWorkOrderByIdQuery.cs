using CleanArchitecture.Cmms.Application.Abstractions.Common;
using CleanArchitecture.Cmms.Application.WorkOrders.Dtos;

namespace CleanArchitecture.Cmms.Application.WorkOrders.Queries.GetWorkOrderById
{
    public sealed record GetWorkOrderByIdQuery(Guid Id) : IQuery<Result<WorkOrderDto>>;

}
