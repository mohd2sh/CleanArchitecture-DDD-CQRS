using CleanArchitecture.Cmms.Application.Abstractions.Persistence;
using CleanArchitecture.Cmms.Domain.Abstractions;
using Microsoft.EntityFrameworkCore.Storage;

namespace CleanArchitecture.Cmms.Infrastructure.Persistence;

internal sealed class EfTransaction : ITransaction
{
    private readonly IDbContextTransaction _tx;
    public EfTransaction(IDbContextTransaction tx) => _tx = tx;
    public Task CommitAsync(CancellationToken cancellationToken = default) => _tx.CommitAsync(cancellationToken);
    public Task RollbackAsync(CancellationToken cancellationToken = default) => _tx.RollbackAsync(cancellationToken);
    public ValueTask DisposeAsync() => _tx.DisposeAsync();
}

internal sealed class EfUnitOfWork : IUnitOfWork
{
    private readonly WriteDbContext _db;
    public EfUnitOfWork(WriteDbContext db) => _db = db;
    public async Task<ITransaction> BeginTransactionAsync(CancellationToken cancellationToken = default) => new EfTransaction(await _db.Database.BeginTransactionAsync(cancellationToken));
    public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default) => _db.SaveChangesAsync(cancellationToken);
    public IReadOnlyCollection<IDomainEvent> CollectDomainEvents() => _db.CollectAndClear();
}
