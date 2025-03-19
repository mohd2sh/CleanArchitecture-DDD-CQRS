using CleanArchitecture.Cmms.Application.Primitives;
using CleanArchitecture.Cmms.Application.WorkOrders.Dtos;
using CleanArchitecture.Cmms.Application.WorkOrders.Interfaces;
using CleanArchitecture.Cmms.Domain.WorkOrders.Enums;
using System.Data;

namespace CleanArchitecture.Cmms.Infrastructure.ReadRepositories.WorkOrders
{
    internal sealed class WorkOrderReadRepository : DapperBaseRepository, IWorkOrderReadRepository
    {
        public WorkOrderReadRepository(IDbConnection connection) : base(connection) { }

        public async Task<WorkOrderDto> GetWorkOrderByIdQuery(Guid id, CancellationToken ct)
        {
            const string sql = "SELECT Id, Title, Status FROM WorkOrders WHERE Id = @Id";

            return await QuerySingleAsync<WorkOrderDto>(sql, ct: ct);
        }

        public async Task<PaginatedList<WorkOrderListItemDto>> GetActiveWithTechnicianAndAssetAsync(PaginationParams pagination, CancellationToken ct)
        {
            const string sql = @"
            SELECT 
                w.Id,
                w.Title,
                ISNULL(t.Name, 'Unassigned') AS TechnicianName,
                ISNULL(a.Name, 'N/A') AS AssetName,
                w.Status
            FROM WorkOrders w
            LEFT JOIN Technicians t ON w.TechnicianId = t.Id
            LEFT JOIN Assets a ON w.Id = a.Id
            WHERE w.Status <> @Completed";

            var param = new { Completed = WorkOrderStatus.Completed.ToString() };

            return await QueryPaginatedAsync<WorkOrderListItemDto>(
                baseSql: sql,
                param: param,
                pageNumber: pagination.PageNumber,
                pageSize: pagination.PageSize,
                orderBy: "w.Id DESC",
                ct: ct);
            //return await QueryAsync<WorkOrderListItemDto>(sql, param, ct);
        }

        //public async Task<WorkOrderDetailDto?> GetDetailAsync(Guid id, CancellationToken ct)
        //{
        //    const string sql = @"
        //    SELECT 
        //        w.Id,
        //        w.Title,
        //        w.Description,
        //        w.Status,
        //        ISNULL(t.Name, 'Unassigned') AS TechnicianName,
        //        ISNULL(a.Name, 'N/A') AS AssetName,
        //        w.CreatedOn,
        //        w.DueDate
        //    FROM WorkOrders w
        //    LEFT JOIN Technicians t ON w.TechnicianId = t.Id
        //    LEFT JOIN Assets a ON w.AssetId = a.Id
        //    WHERE w.Id = @Id";

        //    return await QuerySingleAsync<WorkOrderDetailDto>(sql, new { Id = id }, ct);
        //}
    }
}
