using CleanArchitecture.Core.Application.Abstractions.Persistence.Repositories;
using CleanArchitecture.Core.Application.Abstractions.Query;
using CleanArchitecture.Cmms.Application.WorkOrders.Dtos;

namespace CleanArchitecture.Cmms.Application.WorkOrders.Interfaces
{
    public interface IWorkOrderReadRepository : IReadRepository
    {
        Task<PaginatedList<WorkOrderListItemDto>> GetActiveWithTechnicianAndAssetAsync(PaginationParam pagination, CancellationToken ct);
        Task<WorkOrderDto?> GetWorkOrderById(Guid id, CancellationToken ct);
    }
}
