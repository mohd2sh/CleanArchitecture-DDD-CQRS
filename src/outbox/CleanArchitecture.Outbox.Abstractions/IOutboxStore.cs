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
}
