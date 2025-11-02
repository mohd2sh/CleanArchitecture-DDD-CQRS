using CleanArchitecture.Outbox.IntegrationTests.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace CleanArchitecture.Outbox.IntegrationTests.EdgeCases;

public class LargeBatchTests : OutboxTestBase
{
    public LargeBatchTests(OutboxWebApplicationFactory factory) : base(factory)
    {
    }

    [Fact]
    public async Task MultipleWorkers_LargeBatch_DistributedProcessing()
    {
        // Arrange
        var messageIds = await CreateUnprocessedMessages(500);

        await RunTestProcessorsAsync(workerCount: 4, expectedProcessedCount: 500);

        // Assert
        var processedCount = await GetProcessedCount();
        Assert.Equal(500, processedCount);

        var processedMessages = await DbContext.OutboxMessages
            .Where(m => messageIds.Contains(m.Id) && m.ProcessedAt != null)
            .ToListAsync();
        Assert.Equal(500, processedMessages.Count);
        Assert.Equal(500, processedMessages.Select(m => m.Id).Distinct().Count());
    }
}

