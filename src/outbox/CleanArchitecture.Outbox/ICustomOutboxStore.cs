using System;
using System.Threading;
using System.Threading.Tasks;
using CleanArchitecture.Outbox.Abstractions;

namespace CleanArchitecture.Outbox;
public interface ICustomOutboxStore : IOutboxStore
{
    Task<OutboxMessage?> GetNextUnprocessedMessageAsync(CancellationToken cancellationToken = default);

    Task MarkAsProcessedAsync(Guid messageId, CancellationToken cancellationToken = default);

    Task IncrementRetryAsync(Guid messageId, string error, CancellationToken cancellationToken = default);
}
