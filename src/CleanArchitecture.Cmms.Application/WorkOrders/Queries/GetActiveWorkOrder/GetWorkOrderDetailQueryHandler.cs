using CleanArchitecture.Cmms.Application.Abstractions.Messaging;
using CleanArchitecture.Cmms.Application.Primitives;
using CleanArchitecture.Cmms.Application.WorkOrders.Dtos;
using CleanArchitecture.Cmms.Application.WorkOrders.Interfaces;

namespace CleanArchitecture.Cmms.Application.WorkOrders.Queries.GetActiveWorkOrder
{
    internal sealed class GetActiveWorkOrdersQueryHandler
      : IQueryHandler<GetActiveWorkOrdersQuery, PaginatedList<WorkOrderListItemDto>>
    {
        private readonly IWorkOrderReadRepository _repo;

        public GetActiveWorkOrdersQueryHandler(IWorkOrderReadRepository repo) => _repo = repo;

        public Task<PaginatedList<WorkOrderListItemDto>> Handle(GetActiveWorkOrdersQuery request, CancellationToken ct)
            => _repo.GetActiveWithTechnicianAndAssetAsync(request.Pagination, ct);
    }
}
