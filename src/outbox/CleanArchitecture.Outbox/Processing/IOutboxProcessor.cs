using System.Threading;
using System.Threading.Tasks;

namespace CleanArchitecture.Outbox.Processing;

/// <summary>
/// Processor for outbox messages. Handles the core processing logic.
/// </summary>
public interface IOutboxProcessor
{
    Task ProcessOutboxMessagesAsync(CancellationToken cancellationToken = default);
}

