using CleanArchitecture.Core.Domain.Abstractions;

namespace CleanArchitecture.Core.Application.Abstractions.Persistence;

public interface IUnitOfWork
{
    Task<ITransaction> BeginTransactionAsync(CancellationToken cancellationToken = default);
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
    IReadOnlyCollection<IDomainEvent> CollectDomainEvents();
}
