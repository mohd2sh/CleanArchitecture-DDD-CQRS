using CleanArchitecture.Outbox.IntegrationTests.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace CleanArchitecture.Outbox.IntegrationTests.EdgeCases;

public class ConcurrentAddAndProcessTests : OutboxTestBase
{
    public ConcurrentAddAndProcessTests(OutboxWebApplicationFactory factory) : base(factory)
    {
    }

    [Fact]
    public async Task AddMessages_WhileProcessing_AllProcessed()
    {
        // Arrange
        var initialMessages = await CreateUnprocessedMessages(5);

        // Act
        var (testWorker, completionTask) = await RunTestProcessorWithCompletionAsync(expectedProcessedCount: 10);

        await Task.Delay(200);
        var newMessages = await CreateUnprocessedMessages(5);

        try
        {
            // Wait for completion signal (when all 10 messages processed)
            await completionTask;
        }
        finally
        {
            await testWorker.StopAsync(CancellationToken.None);
            await Task.Delay(100);
        }

        // Assert
        var processedCount = await GetProcessedCount();
        Assert.True(processedCount >= 10);

        // Verify all initial and new messages were processed
        var allMessageIds = initialMessages.Concat(newMessages).ToList();
        var processedMessages = await DbContext.OutboxMessages
            .Where(m => allMessageIds.Contains(m.Id) && m.ProcessedAt != null)
            .ToListAsync();
        Assert.Equal(10, processedMessages.Count);
    }
}

