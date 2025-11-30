using CleanArchitecture.Core.Application.Abstractions.Persistence;
using CleanArchitecture.Core.Domain.Abstractions;
using Microsoft.EntityFrameworkCore;

namespace CleanArchitecture.Core.Infrastructure.Persistence.EfCore;

public class EfUnitOfWork : IUnitOfWork
{
    private readonly DbContext _db;
    public EfUnitOfWork(DbContext db) => _db = db;
    public async Task<ITransaction> BeginTransactionAsync(CancellationToken cancellationToken = default) => new EfTransaction(await _db.Database.BeginTransactionAsync(cancellationToken));
    public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default) => _db.SaveChangesAsync(cancellationToken);
    public IReadOnlyCollection<IDomainEvent> CollectDomainEvents() => _db.CollectAndClear();
}
