using CleanArchitecture.Outbox.IntegrationTests.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace CleanArchitecture.Outbox.IntegrationTests.TransactionHandling;

public class TransactionCommitTests : OutboxTestBase
{
    public TransactionCommitTests(OutboxWebApplicationFactory factory) : base(factory)
    {
    }

    [Fact]
    public async Task SuccessfulProcessing_CommitsTransaction()
    {
        // Arrange
        var message = await CreateTestEventMessage();

        // Act - Process using test BackgroundService with signal-driven completion
        await RunTestProcessorsAsync(workerCount: 1, expectedProcessedCount: 1);

        // Assert
        var updated = await DbContext.OutboxMessages.FindAsync(message.Id);
        Assert.NotNull(updated);
        Assert.NotNull(updated.ProcessedAt);
        Assert.True(updated.ProcessedAt.Value <= DateTime.UtcNow);

        var unprocessed = await DbContext.OutboxMessages
            .Where(m => m.ProcessedAt == null && m.RetryCount < m.MaxRetries)
            .AnyAsync(m => m.Id == message.Id);

        Assert.False(unprocessed);
    }
}

