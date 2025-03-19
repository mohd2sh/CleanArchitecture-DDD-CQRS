using System.Linq.Expressions;

namespace CleanArchitecture.Cmms.Application.Abstractions.Persistence;

public sealed class Criteria<T>
{
    public Expression<Func<T, bool>>? Where { get; init; }
    public List<Expression<Func<T, object>>> Includes { get; } = new();
    public Func<IQueryable<T>, IOrderedQueryable<T>>? OrderBy { get; init; }
    public int? Skip { get; init; }
    public int? Take { get; init; }
    public bool AsNoTracking { get; init; }
}
