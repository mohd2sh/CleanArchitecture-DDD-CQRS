using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace CleanArchitecture.Outbox.Abstractions;

/// <summary>
/// Store for outbox messages providing guaranteed delivery of integration events.
/// </summary>
public interface IOutboxStore
{
    /// <summary>
    /// Adds a new message to the outbox (called within transaction).
    /// </summary>
    Task AddAsync(OutboxMessage message, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets unprocessed messages for background processing.
    /// </summary>
    Task<List<OutboxMessage>> GetUnprocessedAsync(int batchSize = 10, CancellationToken cancellationToken = default);

    /// <summary>
    /// Marks a message as successfully processed.
    /// </summary>
    Task MarkAsProcessedAsync(Guid messageId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Increments retry count and stores error information.
    /// </summary>
    Task IncrementRetryAsync(Guid messageId, string error, CancellationToken cancellationToken = default);
}
