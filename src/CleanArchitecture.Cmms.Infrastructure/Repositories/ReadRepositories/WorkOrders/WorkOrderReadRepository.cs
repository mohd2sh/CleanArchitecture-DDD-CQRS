using System.Data;
using CleanArchitecture.Cmms.Application.WorkOrders.Dtos;
using CleanArchitecture.Cmms.Application.WorkOrders.Interfaces;
using CleanArchitecture.Cmms.Domain.WorkOrders.Enums;
using CleanArchitecture.Core.Application.Abstractions.Query;

namespace CleanArchitecture.Cmms.Infrastructure.Repositories.ReadRepositories.WorkOrders
{
    internal sealed class WorkOrderReadRepository : DapperBaseRepository, IWorkOrderReadRepository
    {
        public WorkOrderReadRepository(IDbConnection connection) : base(connection) { }

        public async Task<WorkOrderDto?> GetWorkOrderById(Guid id, CancellationToken ct)
        {
            const string sql = "SELECT Id, Title, Status FROM workorders.WorkOrders WHERE Id = @Id";

            return await QuerySingleAsync<WorkOrderDto>(sql, param: new { Id = id }, ct: ct);
        }

        public async Task<PaginatedList<WorkOrderListItemDto>> GetActiveWithTechnicianAndAssetAsync(PaginationParam pagination, CancellationToken ct)
        {
            const string sql = @"
            SELECT 
                w.Id,
                w.Title,
                ISNULL(t.Name, 'Unassigned') AS TechnicianName,
                ISNULL(a.Name, 'N/A') AS AssetName,
                w.Status
            FROM workOrders.WorkOrders w
            LEFT JOIN technicians.Technicians t ON w.TechnicianId = t.Id
            LEFT JOIN assets.Assets a ON w.AssetId = a.Id
            WHERE w.Status <> @Completed";

            var param = new { Completed = WorkOrderStatus.Completed.ToString() };

            return await QueryPaginatedAsync<WorkOrderListItemDto>(
                baseSql: sql,
                param: param,
                pageNumber: pagination.PageNumber,
                pageSize: pagination.PageSize,
                orderBy: "w.Id DESC",
                ct: ct);
        }
    }
}
