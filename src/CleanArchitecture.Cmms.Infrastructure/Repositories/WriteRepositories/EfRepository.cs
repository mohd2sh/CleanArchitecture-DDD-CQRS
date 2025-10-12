using CleanArchitecture.Cmms.Application.Abstractions.Persistence;
using CleanArchitecture.Cmms.Application.Abstractions.Persistence.Repositories;
using CleanArchitecture.Cmms.Domain.Abstractions;
using CleanArchitecture.Cmms.Infrastructure.Persistence.EfCore;
using Microsoft.EntityFrameworkCore;

namespace CleanArchitecture.Cmms.Infrastructure.Repositories.WriteRepositories;

internal class EfRepository<T, TId> : IRepository<T, TId> where T : Entity<TId>, IAggregateRoot
{
    private readonly WriteDbContext _db;
    public EfRepository(WriteDbContext db) => _db = db;
    public async Task<T?> GetByIdAsync(TId id, CancellationToken cancellationToken = default) => await _db.Set<T>().FindAsync([id], cancellationToken);
    public Task AddAsync(T entity, CancellationToken cancellationToken = default) { _db.Set<T>().Add(entity); return Task.CompletedTask; }
    public Task AddRangeAsync(IEnumerable<T> entities, CancellationToken cancellationToken = default) { _db.Set<T>().AddRange(entities); return Task.CompletedTask; }
    public Task RemoveAsync(T entity, CancellationToken cancellationToken = default) { _db.Set<T>().Remove(entity); return Task.CompletedTask; }
    public Task UpdateAsync(T entity, CancellationToken cancellationToken = default) { _db.Set<T>().Update(entity); return Task.CompletedTask; }

    public async Task<T?> FirstOrDefaultAsync(Criteria<T> c, CancellationToken cancellationToken = default) => await _db.Set<T>().AsQueryable().Apply(c).FirstOrDefaultAsync(cancellationToken);
    public async Task<IReadOnlyList<T>> ListAsync(Criteria<T> c, CancellationToken cancellationToken = default) => await _db.Set<T>().AsQueryable().Apply(c).ToListAsync(cancellationToken);
    public async Task<bool> AnyAsync(Criteria<T> c, CancellationToken cancellationToken = default) => await _db.Set<T>().AsQueryable().Apply(c).AnyAsync(cancellationToken);

}
