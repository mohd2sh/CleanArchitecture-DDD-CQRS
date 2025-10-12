using CleanArchitecture.Cmms.Application.Abstractions.Persistence;
using CleanArchitecture.Cmms.Domain.Abstractions;

namespace CleanArchitecture.Cmms.Infrastructure.Persistence.EfCore;

internal sealed class EfUnitOfWork : IUnitOfWork
{
    private readonly WriteDbContext _db;
    public EfUnitOfWork(WriteDbContext db) => _db = db;
    public async Task<ITransaction> BeginTransactionAsync(CancellationToken cancellationToken = default) => new EfTransaction(await _db.Database.BeginTransactionAsync(cancellationToken));
    public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default) => _db.SaveChangesAsync(cancellationToken);
    public IReadOnlyCollection<IDomainEvent> CollectDomainEvents() => _db.CollectAndClear();
}
