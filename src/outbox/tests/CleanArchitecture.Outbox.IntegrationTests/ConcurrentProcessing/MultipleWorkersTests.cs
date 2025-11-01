using CleanArchitecture.Outbox.IntegrationTests.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace CleanArchitecture.Outbox.IntegrationTests.ConcurrentProcessing;

public class MultipleWorkersTests : OutboxTestBase
{
    public MultipleWorkersTests(OutboxWebApplicationFactory factory) : base(factory)
    {
    }

    [Fact]
    public async Task MultipleWorkers_ShouldNotProcessSameMessage()
    {
        // Arrange
        var messageIds = await CreateUnprocessedMessages(10);

        // Act
        await RunTestProcessorsAsync(workerCount: 2, expectedProcessedCount: 10);

        // Assert
        var processedCount = await GetProcessedCount();
        Assert.Equal(10, processedCount);

        var processedMessages = await DbContext.OutboxMessages
            .Where(m => messageIds.Contains(m.Id) && m.ProcessedAt != null)
            .ToListAsync();
        Assert.Equal(10, processedMessages.Count);
    }

    [Fact]
    public async Task MultipleWorkers_WithManyMessages_ProcessAllCorrectly()
    {
        // Arrange
        var messageIds = await CreateUnprocessedMessages(100);

        // Act 
        await RunTestProcessorsAsync(workerCount: 4, expectedProcessedCount: 100);

        // Assert
        var processedCount = await GetProcessedCount();
        Assert.Equal(100, processedCount);

        var processedMessages = await DbContext.OutboxMessages
            .Where(m => messageIds.Contains(m.Id) && m.ProcessedAt != null)
            .ToListAsync();
        Assert.Equal(100, processedMessages.Count);
    }

    [Fact]
    public async Task Workers_GetDifferentMessages_WithREADPAST()
    {
        // Arrange
        var messageIds = await CreateUnprocessedMessages(5);

        // Act
        await RunTestProcessorsAsync(workerCount: 2, expectedProcessedCount: 5);

        // Assert
        var processedCount = await GetProcessedCount();
        Assert.Equal(5, processedCount);

        var processedMessages = await DbContext.OutboxMessages
            .Where(m => messageIds.Contains(m.Id) && m.ProcessedAt != null)
            .ToListAsync();
        Assert.Equal(5, processedMessages.Count);
        Assert.Equal(5, processedMessages.Select(m => m.Id).Distinct().Count());
    }
}

