using CleanArchitecture.Outbox.Abstractions;
using CleanArchitecture.Outbox.Persistence;

namespace CleanArchitecture.Outbox.IntegrationTests.Infrastructure.TestHelpers;

public static class AssertHelpers
{
    public static void AssertDeadLetterMessage(DeadLetterMessage deadLetter, OutboxMessage originalMessage)
    {
        Assert.Equal(originalMessage.Id, deadLetter.Id);
        Assert.Equal(originalMessage.EventType, deadLetter.EventType);
        Assert.Equal(originalMessage.Payload, deadLetter.Payload);
        Assert.Equal(originalMessage.CreatedAt, deadLetter.CreatedAt);
        Assert.Equal(originalMessage.RetryCount, deadLetter.RetryCount);
        Assert.Equal(originalMessage.LastError, deadLetter.LastError);
        Assert.Equal(originalMessage.MaxRetries, deadLetter.MaxRetries);
        Assert.True(deadLetter.MovedToDeadLetterAt > DateTime.UtcNow.AddMinutes(-1));
    }
}

