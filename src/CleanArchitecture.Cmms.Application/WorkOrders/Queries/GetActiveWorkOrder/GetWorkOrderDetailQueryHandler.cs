using CleanArchitecture.Cmms.Application.Abstractions.Common;
using CleanArchitecture.Cmms.Application.Abstractions.Query;
using CleanArchitecture.Cmms.Application.WorkOrders.Dtos;
using CleanArchitecture.Cmms.Application.WorkOrders.Interfaces;

namespace CleanArchitecture.Cmms.Application.WorkOrders.Queries.GetActiveWorkOrder
{
    internal sealed class GetActiveWorkOrdersQueryHandler
      : IQueryHandler<GetActiveWorkOrdersQuery, Result<PaginatedList<WorkOrderListItemDto>>>
    {
        private readonly IWorkOrderReadRepository _repo;

        public GetActiveWorkOrdersQueryHandler(IWorkOrderReadRepository repo) => _repo = repo;

        public async Task<Result<PaginatedList<WorkOrderListItemDto>>> Handle(GetActiveWorkOrdersQuery request, CancellationToken cancellationToken)
        {
            return await _repo.GetActiveWithTechnicianAndAssetAsync(request.Pagination, cancellationToken);
        }
    }
}
