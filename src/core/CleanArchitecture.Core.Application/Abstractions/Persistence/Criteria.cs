using System.Linq.Expressions;

namespace CleanArchitecture.Core.Application.Abstractions.Persistence;

public sealed class Criteria<T>
{
    public Expression<Func<T, bool>>? Where { get; init; }
    public List<Expression<Func<T, object>>> Includes { get; init; } = new();
    public Func<IQueryable<T>, IOrderedQueryable<T>>? OrderBy { get; init; }
    public int? Skip { get; init; }
    public int? Take { get; init; }

    public static CriteriaBuilder<T> New() => new();
}
