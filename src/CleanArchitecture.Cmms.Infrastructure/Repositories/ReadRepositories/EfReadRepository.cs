using CleanArchitecture.Cmms.Infrastructure.Persistence.EfCore;
using CleanArchitecture.Core.Application.Abstractions.Persistence;
using CleanArchitecture.Core.Application.Abstractions.Persistence.Repositories;
using CleanArchitecture.Core.Application.Abstractions.Query;
using CleanArchitecture.Core.Domain.Abstractions;
using Microsoft.EntityFrameworkCore;

namespace CleanArchitecture.Cmms.Infrastructure.Repositories.ReadRepositories
{

    /// <summary>
    /// Usage for quires against single aggregate , for simplicity and avoid duplicate code for single entity.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <typeparam name="TId"></typeparam>
    public class EfReadRepository<T, TId> : IReadRepository<T, TId>
        where T : class, IEntity<TId>
    {
        private readonly ReadDbContext _db;

        public EfReadRepository(ReadDbContext db) => _db = db;

        public async Task<T?> FirstOrDefaultAsync(Criteria<T> criteria, CancellationToken cancellationToken = default)
            => await _db.Set<T>().AsQueryable().Apply(criteria).AsNoTracking().FirstOrDefaultAsync(cancellationToken);

        public async Task<bool> AnyAsync(Criteria<T> criteria, CancellationToken cancellationToken = default)
            => await _db.Set<T>().AsQueryable().Apply(criteria).AsNoTracking().AnyAsync(cancellationToken);

        public async Task<PaginatedList<T>> ListAsync(Criteria<T> criteria, CancellationToken cancellationToken = default)
        {
            var query = _db.Set<T>().AsQueryable().Apply(criteria).AsNoTracking();

            var totalCount = await query.CountAsync(cancellationToken);

            var items = await query.ToListAsync(cancellationToken);

            return PaginatedList<T>.CreateFromOffset(items, totalCount, criteria.Skip, criteria.Take);
        }
    }
}
