using System.Linq.Expressions;

namespace CleanArchitecture.Core.Application.Abstractions.Persistence;

public sealed class CriteriaBuilder<T>
{
    private Expression<Func<T, bool>>? _where;
    private readonly List<Expression<Func<T, object>>> _includes = new();
    private Func<IQueryable<T>, IOrderedQueryable<T>>? _orderBy;
    private int? _skip;
    private int? _take;

    public CriteriaBuilder<T> Where(Expression<Func<T, bool>> predicate)
    {
        _where = predicate;
        return this;
    }

    public CriteriaBuilder<T> Include(Expression<Func<T, object>> include)
    {
        _includes.Add(include);
        return this;
    }

    public CriteriaBuilder<T> OrderByAsc<TKey>(Expression<Func<T, TKey>> keySelector)
    {
        _orderBy = q => q.OrderBy(keySelector);
        return this;
    }

    public CriteriaBuilder<T> OrderByDesc<TKey>(Expression<Func<T, TKey>> keySelector)
    {
        _orderBy = q => q.OrderByDescending(keySelector);
        return this;
    }

    public CriteriaBuilder<T> Skip(int skip)
    {
        _skip = skip;
        return this;
    }

    public CriteriaBuilder<T> Take(int take)
    {
        _take = take;
        return this;
    }

    public Criteria<T> Build() => new()
    {
        Where = _where,
        OrderBy = _orderBy,
        Skip = _skip,
        Take = _take,
        Includes = _includes.ToList()
    };
}
