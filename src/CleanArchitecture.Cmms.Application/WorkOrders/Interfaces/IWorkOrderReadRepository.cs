using CleanArchitecture.Cmms.Application.Abstractions.Persistence;
using CleanArchitecture.Cmms.Application.Primitives;
using CleanArchitecture.Cmms.Application.WorkOrders.Dtos;

namespace CleanArchitecture.Cmms.Application.WorkOrders.Interfaces
{
    public interface IWorkOrderReadRepository : IReadRepository
    {
        Task<PaginatedList<WorkOrderListItemDto>> GetActiveWithTechnicianAndAssetAsync(PaginationParams pagination, CancellationToken ct);
        Task<WorkOrderDto> GetWorkOrderByIdQuery(Guid id, CancellationToken ct);
    }
}
