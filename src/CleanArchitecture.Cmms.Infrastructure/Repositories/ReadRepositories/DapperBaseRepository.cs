using System.Data;
using CleanArchitecture.Core.Application.Abstractions.Query;
using Dapper;

namespace CleanArchitecture.Cmms.Infrastructure.Repositories.ReadRepositories
{
    internal abstract class DapperBaseRepository
    {
        protected readonly IDbConnection _connection;

        protected DapperBaseRepository(IDbConnection connection)
            => _connection = connection;

        protected async Task<IReadOnlyList<T>> QueryAsync<T>(string sql, object? param = null, CancellationToken ct = default)
        {
            var result = await _connection.QueryAsync<T>(new CommandDefinition(sql, param, cancellationToken: ct));
            return result.ToList();
        }

        protected async Task<T?> QuerySingleAsync<T>(string sql, object? param = null, CancellationToken ct = default)
        {
            return await _connection.QuerySingleOrDefaultAsync<T>(new CommandDefinition(sql, param, cancellationToken: ct));
        }

        protected async Task<PaginatedList<T>> QueryPaginatedAsync<T>(
         string baseSql,
         object? param,
         int pageNumber,
         int pageSize,
         string? orderBy = null,
         CancellationToken ct = default)
        {
            if (pageNumber < 1) pageNumber = 1;
            if (pageSize < 1) pageSize = 20;

            var skip = (pageNumber - 1) * pageSize;

            // Total count query
            var countSql = $"SELECT COUNT(1) FROM ({baseSql}) AS CountQuery";

            // Append pagination
            var orderedSql = !string.IsNullOrWhiteSpace(orderBy)
                ? $"{baseSql} ORDER BY {orderBy}"
                : $"{baseSql} ORDER BY (SELECT NULL)"; // SQL Server requires ORDER BY for OFFSET/FETCH

            var pagedSql = $"{orderedSql} OFFSET @Skip ROWS FETCH NEXT @PageSize ROWS ONLY;";

            var dynamicParams = new DynamicParameters(param);
            dynamicParams.Add("Skip", skip);
            dynamicParams.Add("PageSize", pageSize);

            // Execute both queries in a single round-trip
            using var multi = await _connection.QueryMultipleAsync(
                new CommandDefinition($"{countSql}; {pagedSql}", dynamicParams, cancellationToken: ct));

            var totalCount = await multi.ReadFirstAsync<int>();
            var items = (await multi.ReadAsync<T>()).ToList();

            return PaginatedList<T>.Create(items, totalCount, pageNumber, pageSize);
        }
    }
}
