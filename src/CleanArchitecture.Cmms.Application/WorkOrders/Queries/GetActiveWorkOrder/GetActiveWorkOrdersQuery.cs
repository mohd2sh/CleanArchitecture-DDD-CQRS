using CleanArchitecture.Cmms.Application.WorkOrders.Dtos;

namespace CleanArchitecture.Cmms.Application.WorkOrders.Queries.GetActiveWorkOrder
{
    public sealed record GetActiveWorkOrdersQuery(PaginationParams Pagination)
    : IQuery<PaginatedList<WorkOrderListItemDto>>;

}
