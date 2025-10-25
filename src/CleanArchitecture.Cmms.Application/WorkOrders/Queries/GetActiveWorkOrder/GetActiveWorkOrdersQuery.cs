using CleanArchitecture.Cmms.Application.WorkOrders.Dtos;
using CleanArchitecture.Core.Application.Abstractions.Common;
using CleanArchitecture.Core.Application.Abstractions.Query;

namespace CleanArchitecture.Cmms.Application.WorkOrders.Queries.GetActiveWorkOrder
{
    public sealed record GetActiveWorkOrdersQuery(PaginationParam Pagination)
    : IQuery<Result<PaginatedList<WorkOrderListItemDto>>>;

}
