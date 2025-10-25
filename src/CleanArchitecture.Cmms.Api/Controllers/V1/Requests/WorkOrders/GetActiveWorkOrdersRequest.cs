using CleanArchitecture.Cmms.Application.Abstractions.Query;

namespace CleanArchitecture.Cmms.Api.Controllers.V1.Requests.WorkOrders
{
    public sealed record GetActiveWorkOrdersRequest(PaginationParam Pagination);
}
