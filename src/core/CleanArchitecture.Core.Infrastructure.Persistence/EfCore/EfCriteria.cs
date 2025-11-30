using CleanArchitecture.Core.Application.Abstractions.Persistence;
using Microsoft.EntityFrameworkCore;

namespace CleanArchitecture.Core.Infrastructure.Persistence.EfCore;

public static class EfCriteria
{
    public static IQueryable<T> Apply<T>(this IQueryable<T> query, Criteria<T> criteria) where T : class
    {
        if (criteria.Where is not null) query = query.Where(criteria.Where);

        foreach (var i in criteria.Includes) query = query.Include(i);

        if (criteria.OrderBy is not null) query = criteria.OrderBy(query);

        if (criteria.Skip.HasValue) query = query.Skip(criteria.Skip.Value);

        if (criteria.Take.HasValue) query = query.Take(criteria.Take.Value);

        return query;
    }

    public static async Task<(IQueryable<T> Query, int TotalCount)> ApplyWithCountAsync<T>(this IQueryable<T> query,
        Criteria<T> criteria,
        CancellationToken cancellationToken = default) where T : class
    {
        if (criteria.Where is not null)
            query = query.Where(criteria.Where);

        foreach (var include in criteria.Includes)
            query = query.Include(include);

        var totalCount = await query.CountAsync(cancellationToken);

        if (criteria.OrderBy is not null)
            query = criteria.OrderBy(query);

        if (criteria.Skip.HasValue)
            query = query.Skip(criteria.Skip.Value);

        if (criteria.Take.HasValue)
            query = query.Take(criteria.Take.Value);

        return (query, totalCount);
    }
}
