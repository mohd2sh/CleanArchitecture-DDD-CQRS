using CleanArchitecture.Outbox.IntegrationTests.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace CleanArchitecture.Outbox.IntegrationTests.ConcurrentProcessing;

public class RaceConditionTests : OutboxTestBase
{
    public RaceConditionTests(OutboxWebApplicationFactory factory) : base(factory)
    {
    }

    [Fact]
    public async Task ConcurrentWorkers_NoRaceCondition()
    {
        // Arrange
        var messageIds = await CreateUnprocessedMessages(20);

        // Act
        await RunTestProcessorsAsync(workerCount: 3, expectedProcessedCount: 20);

        // Assert
        var processedCount = await GetProcessedCount();
        Assert.Equal(20, processedCount);

        // Verify all messages processed exactly once (no race conditions)
        var processedMessages = await DbContext.OutboxMessages
            .Where(m => messageIds.Contains(m.Id) && m.ProcessedAt != null)
            .ToListAsync();
        Assert.Equal(20, processedMessages.Count);
        Assert.Equal(20, processedMessages.Select(m => m.Id).Distinct().Count());
    }

    [Fact]
    public async Task WorkerStarts_WhileAnotherProcessing()
    {
        // Arrange
        var messageIds = await CreateUnprocessedMessages(5);

        // Act
        await RunTestProcessorsAsync(workerCount: 2, expectedProcessedCount: 5);

        // Assert
        var processedCount = await GetProcessedCount();
        Assert.Equal(5, processedCount);

        // Verify all messages processed (HOLDLOCK prevented conflicts)
        var processedMessages = await DbContext.OutboxMessages
            .Where(m => messageIds.Contains(m.Id) && m.ProcessedAt != null)
            .ToListAsync();
        Assert.Equal(5, processedMessages.Count);
    }
}

