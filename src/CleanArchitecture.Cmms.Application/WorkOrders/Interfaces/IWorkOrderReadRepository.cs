using CleanArchitecture.Cmms.Application.Abstractions.Persistence.Repositories;
using CleanArchitecture.Cmms.Application.Primitives;
using CleanArchitecture.Cmms.Application.WorkOrders.Dtos;

namespace CleanArchitecture.Cmms.Application.WorkOrders.Interfaces
{
    public interface IWorkOrderReadRepository : IReadRepository
    {
        Task<PaginatedList<WorkOrderListItemDto>> GetActiveWithTechnicianAndAssetAsync(PaginationParam pagination, CancellationToken ct);
        Task<WorkOrderDto?> GetWorkOrderByIdQuery(Guid id, CancellationToken ct);
    }
}
