using CleanArchitecture.Cmms.Application.Abstractions.Persistence;
using Microsoft.EntityFrameworkCore;

namespace CleanArchitecture.Cmms.Infrastructure.Persistence.EfCore;

internal static class EfCriteria
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
}
