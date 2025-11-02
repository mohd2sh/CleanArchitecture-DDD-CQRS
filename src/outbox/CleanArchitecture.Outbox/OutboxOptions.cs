namespace CleanArchitecture.Outbox;

/// <summary>
/// Configuration options for the outbox pattern implementation.
/// </summary>
public sealed class OutboxOptions
{
    /// <summary>
    /// Number of parallel worker instances to run within the same process.
    /// Default: 2
    /// </summary>
    public int WorkerCount { get; set; } = 2;
}

