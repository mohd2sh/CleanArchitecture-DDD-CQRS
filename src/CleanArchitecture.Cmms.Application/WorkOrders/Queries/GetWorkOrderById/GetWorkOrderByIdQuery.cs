using CleanArchitecture.Cmms.Application.WorkOrders.Dtos;
using CleanArchitecture.Core.Application.Abstractions.Common;

namespace CleanArchitecture.Cmms.Application.WorkOrders.Queries.GetWorkOrderById
{
    public sealed record GetWorkOrderByIdQuery(Guid Id) : IQuery<Result<WorkOrderDto>>;

}
