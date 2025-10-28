using CleanArchitecture.Core.Application.Abstractions.Query;

namespace CleanArchitecture.Core.Application.Abstractions.Persistence.Repositories;

/// <summary>
/// Marker and a base for any custom read repository.
/// </summary>
public interface IReadRepository { }

/// <summary>
/// Generic read-only repository abstraction for querying a single aggregate entity.
/// Provides common query operations such as retrieval by ID, filtering, and pagination using <see cref="Criteria{T}"/>.
/// Intended for simple, entity-focused read scenarios. For more complex or cross-domain queries and partial DTO mappings.
/// implement a custom repository using the <see cref="IReadRepository"/> marker interface
/// </summary>
/// <typeparam name="T">The entity type.</typeparam>
/// <typeparam name="TId">The type of the entity's identifier.</typeparam>

public interface IReadRepository<T, TId> : IReadRepository
{
    Task<T?> FirstOrDefaultAsync(Criteria<T> criteria, CancellationToken cancellationToken = default);
    Task<bool> AnyAsync(Criteria<T> criteria, CancellationToken cancellationToken = default);
    Task<PaginatedList<T>> ListAsync(Criteria<T> criteria, CancellationToken cancellationToken = default);
}
