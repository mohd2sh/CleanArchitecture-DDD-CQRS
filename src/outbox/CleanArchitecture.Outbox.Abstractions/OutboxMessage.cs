using System;

namespace CleanArchitecture.Outbox.Abstractions;

/// <summary>
/// Represents an integration event stored in the outbox
/// </summary>
public sealed class OutboxMessage
{
    public Guid Id { get; set; }
    public string EventType { get; set; } = default!;
    public string Payload { get; set; } = default!;
    public DateTime CreatedAt { get; set; }
    public DateTime? ProcessedAt { get; set; }
    public int RetryCount { get; set; }
    public string? LastError { get; set; }
    public int MaxRetries { get; set; } = 3;

    //TODO: Add additional information dict and maybe include operationId, CorrelationId?
}
