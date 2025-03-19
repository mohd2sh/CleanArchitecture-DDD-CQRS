using CleanArchitecture.Cmms.Application.Abstractions.Persistence;
using CleanArchitecture.Cmms.Domain.Abstractions;
using Microsoft.EntityFrameworkCore;

namespace CleanArchitecture.Cmms.Infrastructure.Persistence;

internal class EfRepository<T, TId> : IRepository<T, TId> where T : class, IAggregateRoot
{
    private readonly WriteDbContext _db;
    public EfRepository(WriteDbContext db) => _db = db;
    public async Task<T?> GetByIdAsync(TId id, CancellationToken ct = default) => await _db.Set<T>().FindAsync([id], ct);
    public Task AddAsync(T entity, CancellationToken ct = default) { _db.Set<T>().Add(entity); return Task.CompletedTask; }
    public Task AddRangeAsync(IEnumerable<T> entities, CancellationToken ct = default) { _db.Set<T>().AddRange(entities); return Task.CompletedTask; }
    public Task RemoveAsync(T entity, CancellationToken ct = default) { _db.Set<T>().Remove(entity); return Task.CompletedTask; }
    public Task UpdateAsync(T entity, CancellationToken ct = default) { _db.Set<T>().Update(entity); return Task.CompletedTask; }

    public async Task<T?> FirstOrDefaultAsync(Criteria<T> c, CancellationToken ct = default) => await EfCriteria.Apply(_db.Set<T>().AsQueryable(), c).FirstOrDefaultAsync(ct);
    public async Task<IReadOnlyList<T>> ListAsync(Criteria<T> c, CancellationToken ct = default) => await EfCriteria.Apply(_db.Set<T>().AsQueryable(), c).ToListAsync(ct);
    public async Task<bool> AnyAsync(Criteria<T> c, CancellationToken ct = default) => await EfCriteria.Apply(_db.Set<T>().AsQueryable(), c).AnyAsync(ct);

}
