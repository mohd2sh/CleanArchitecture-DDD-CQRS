using CleanArchitecture.Cmms.Application.Abstractions.Persistence;
using CleanArchitecture.Cmms.Application.Abstractions.Persistence.Repositories;
using CleanArchitecture.Cmms.Application.Abstractions.Query;
using CleanArchitecture.Cmms.Infrastructure.Persistence.EfCore;
using Microsoft.EntityFrameworkCore;

namespace CleanArchitecture.Cmms.Infrastructure.Repositories.ReadRepositories
{

    /// <summary>
    /// Usage for quires against single aggregate , for simplicity and avoid duplicate code for single entity.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <typeparam name="TId"></typeparam>
    public class EfReadRepository<T, TId> : IReadRepository<T, TId>
        where T : class
    {
        private readonly ReadDbContext _db;

        public EfReadRepository(ReadDbContext db) => _db = db;

        public async Task<T?> GetByIdAsync(TId id, CancellationToken cancellationToken = default)
            => await _db.Set<T>().FindAsync([id], cancellationToken);

        public async Task<T?> FirstOrDefaultAsync(Criteria<T> criteria, CancellationToken cancellationToken = default)
            => await _db.Set<T>().AsQueryable().Apply(criteria).FirstOrDefaultAsync(cancellationToken);

        public async Task<bool> AnyAsync(Criteria<T> criteria, CancellationToken cancellationToken = default)
            => await _db.Set<T>().AsQueryable().Apply(criteria).AnyAsync(cancellationToken);

        public async Task<PaginatedList<T>> ListAsync(Criteria<T> criteria, CancellationToken cancellationToken = default)
        {
            var query = _db.Set<T>().AsQueryable().Apply(criteria);

            var totalCount = await query.CountAsync(cancellationToken);

            var items = await query.ToListAsync(cancellationToken);

            return PaginatedList<T>.CreateFromOffset(items, totalCount, criteria.Skip, criteria.Take);
        }
    }
}
