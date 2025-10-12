using CleanArchitecture.Cmms.Application.Abstractions.Persistence;
using Microsoft.EntityFrameworkCore.Storage;

namespace CleanArchitecture.Cmms.Infrastructure.Persistence.EfCore;

internal sealed class EfTransaction : ITransaction
{
    private readonly IDbContextTransaction _tx;
    public EfTransaction(IDbContextTransaction tx) => _tx = tx;
    public Task CommitAsync(CancellationToken cancellationToken = default) => _tx.CommitAsync(cancellationToken);
    public Task RollbackAsync(CancellationToken cancellationToken = default) => _tx.RollbackAsync(cancellationToken);
    public ValueTask DisposeAsync() => _tx.DisposeAsync();
}
