using CleanArchitecture.Cmms.Domain.Abstractions;

namespace CleanArchitecture.Cmms.Application.Abstractions.Persistence;

/// <summary>
/// Marker and a base for any custom read repo.
/// </summary>
public interface IReadRepository { }


//Marker and base for any transactional repo.
public interface IRepository<T> { }
public interface IRepository<T, TId> : IRepository<T> where T : class, IAggregateRoot
{
    Task<T?> GetByIdAsync(TId id, CancellationToken cancellationToken = default);
    Task<T?> FirstOrDefaultAsync(Criteria<T> c, CancellationToken cancellationToken = default);
    Task AddAsync(T entity, CancellationToken cancellationToken = default);
    Task AddRangeAsync(IEnumerable<T> entities, CancellationToken cancellationToken = default);
    Task RemoveAsync(T entity, CancellationToken cancellationToken = default);
    Task UpdateAsync(T entity, CancellationToken cancellationToken = default);
}
