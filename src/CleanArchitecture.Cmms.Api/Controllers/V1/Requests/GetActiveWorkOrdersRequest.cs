using CleanArchitecture.Cmms.Application.Primitives;

namespace CleanArchitecture.Cmms.Api.Controllers.V1.Requests
{
    public sealed record GetActiveWorkOrdersRequest(PaginationParams Pagination);
}
