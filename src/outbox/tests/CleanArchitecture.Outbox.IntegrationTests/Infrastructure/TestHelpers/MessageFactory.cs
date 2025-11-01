using System.Text.Json;
using CleanArchitecture.Outbox.Abstractions;

namespace CleanArchitecture.Outbox.IntegrationTests.Infrastructure.TestHelpers;

public static class MessageFactory
{
    public static OutboxMessage CreateTestMessage(
        string eventType = "TestEvent",
        object payload = null,
        int maxRetries = 3,
        int retryCount = 0,
        DateTime? processedAt = null)
    {
        return new OutboxMessage
        {
            Id = Guid.NewGuid(),
            EventType = eventType,
            Payload = payload != null ? JsonSerializer.Serialize(payload) : "{}",
            CreatedAt = DateTime.UtcNow,
            ProcessedAt = processedAt,
            RetryCount = retryCount,
            MaxRetries = maxRetries
        };
    }

    public static OutboxMessage CreateMessageAtMaxRetries(int maxRetries = 3)
    {
        return CreateTestMessage(retryCount: maxRetries - 1, maxRetries: maxRetries);
    }
}
