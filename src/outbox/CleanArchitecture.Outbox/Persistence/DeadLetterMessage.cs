using System;

namespace CleanArchitecture.Outbox.Persistence;

public sealed class DeadLetterMessage
{
    public Guid Id { get; set; }
    public string EventType { get; set; } = default!;
    public string Payload { get; set; } = default!;
    public DateTime CreatedAt { get; set; }
    public DateTime MovedToDeadLetterAt { get; set; }
    public int RetryCount { get; set; }
    public string? LastError { get; set; }
    public int MaxRetries { get; set; }
}

