using CleanArchitecture.Outbox.Abstractions;
using CleanArchitecture.Outbox.IntegrationTests.Infrastructure;
using CleanArchitecture.Outbox.IntegrationTests.Infrastructure.TestHelpers;
using Microsoft.EntityFrameworkCore;

namespace CleanArchitecture.Outbox.IntegrationTests.DeadLetterQueue;

public class DeadLetterTests : OutboxTestBase
{
    public DeadLetterTests(OutboxWebApplicationFactory factory) : base(factory)
    {
    }

    [Fact]
    public async Task MessageExceedingMaxRetries_MovedToDeadLetter()
    {
        // Arrange
        var message = MessageFactory.CreateMessageAtMaxRetries(maxRetries: 3);
        await OutboxStore.AddAsync(message);

        // Act - Increment retry to exceed max
        await OutboxStore.IncrementRetryAsync(message.Id, "Test error");

        // Assert
        var deadLetterMessages = await GetDeadLetterMessages();
        Assert.Single(deadLetterMessages);

        var deadLetter = deadLetterMessages[0];
        AssertHelpers.AssertDeadLetterMessage(deadLetter, message);
        Assert.Equal(3, deadLetter.RetryCount);

        // Verify message removed from OutboxMessages
        var inOutbox = await DbContext.OutboxMessages
            .FirstOrDefaultAsync(m => m.Id == message.Id);
        Assert.Null(inOutbox);
    }

    [Fact]
    public async Task DeadLetterMessage_HasCorrectData()
    {
        // Arrange
        var originalMessage = new OutboxMessage
        {
            Id = Guid.NewGuid(),
            EventType = "TestEvent",
            Payload = "{\"test\":\"data\"}",
            CreatedAt = DateTime.UtcNow.AddMinutes(-10),
            ProcessedAt = null,
            RetryCount = 2,
            LastError = "Previous error",
            MaxRetries = 3
        };
        await OutboxStore.AddAsync(originalMessage);

        // Act
        await OutboxStore.IncrementRetryAsync(originalMessage.Id, "Final error");

        // Assert
        var deadLetterMessages = await GetDeadLetterMessages();
        var deadLetter = deadLetterMessages[0];

        AssertHelpers.AssertDeadLetterMessage(deadLetter, originalMessage);
        Assert.Equal("Final error", deadLetter.LastError);
        Assert.True(deadLetter.MovedToDeadLetterAt > DateTime.UtcNow.AddMinutes(-1));
    }

    [Fact]
    public async Task MoveToDeadLetter_BatchedWithRetryIncrement()
    {
        // Arrange
        var message = MessageFactory.CreateMessageAtMaxRetries(maxRetries: 3);
        await OutboxStore.AddAsync(message);

        // Act
        await OutboxStore.IncrementRetryAsync(message.Id, "Test error");

        // Assert
        var deadLetter = await DbContext.DeadLetterMessages
            .FirstOrDefaultAsync(d => d.Id == message.Id);

        Assert.NotNull(deadLetter);
        Assert.Equal(3, deadLetter.RetryCount);

        // Verify atomicity - message should not exist in OutboxMessages
        var stillInOutbox = await DbContext.OutboxMessages
            .AnyAsync(m => m.Id == message.Id);
        Assert.False(stillInOutbox);
    }
}

