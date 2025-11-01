using System.Text.Json;
using CleanArchitecture.Core.Application.Abstractions.Events;
using CleanArchitecture.Core.Domain.Abstractions;
using CleanArchitecture.Outbox.Abstractions;
using CleanArchitecture.Outbox.Persistence;
using CleanArchitecture.Outbox.Processing;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Respawn;

namespace CleanArchitecture.Outbox.IntegrationTests.Infrastructure;

public abstract class OutboxTestBase : IClassFixture<OutboxWebApplicationFactory>, IAsyncLifetime
{
    private static Respawner _respawner;
    private static readonly SemaphoreSlim _respawnerLock = new(1, 1);
    private readonly OutboxWebApplicationFactory _factory;

    private IServiceScope _scope;
    protected OutboxDbContext DbContext { get; private set; } = null!;
    protected ICustomOutboxStore OutboxStore { get; private set; } = null!;
    protected IServiceProvider ServiceProvider { get; private set; } = null!;

    protected OutboxTestBase(OutboxWebApplicationFactory factory)
    {
        _factory = factory;
    }

    public async Task InitializeAsync()
    {
        await DbRespawner();

        _scope = _factory.Services.CreateScope();
        ServiceProvider = _scope.ServiceProvider;
        DbContext = _scope.ServiceProvider.GetRequiredService<OutboxDbContext>();
        OutboxStore = _scope.ServiceProvider.GetRequiredService<ICustomOutboxStore>();
    }

    private async Task DbRespawner()
    {
        if (_respawner == null)
        {
            await _respawnerLock.WaitAsync();
            try
            {
                if (_respawner == null)
                {
                    await using var conn = new SqlConnection(_factory.ConnectionString);
                    await conn.OpenAsync();
                    _respawner = await Respawner.CreateAsync(conn, new RespawnerOptions
                    {
                        DbAdapter = DbAdapter.SqlServer,
                        TablesToIgnore = ["__EFMigrationsHistory"]
                    });
                }
            }
            finally
            {
                _respawnerLock.Release();
            }
        }

        await using (var conn = new SqlConnection(_factory.ConnectionString))
        {
            await conn.OpenAsync();
            await _respawner.ResetAsync(conn);
        }
    }

    public async Task DisposeAsync()
    {
        _scope?.Dispose();
        await Task.CompletedTask;
    }

    protected async Task<List<Guid>> CreateUnprocessedMessages(int count, int maxRetries = 3)
    {
        var messageIds = new List<Guid>();

        for (int i = 0; i < count; i++)
        {
            var testEvent = new TestDomainEvent { Data = $"Value{i}" };
            var message = new OutboxMessage
            {
                Id = Guid.NewGuid(),
                EventType = typeof(TestDomainEvent).AssemblyQualifiedName!,
                Payload = JsonSerializer.Serialize(testEvent),
                CreatedAt = DateTime.UtcNow,
                ProcessedAt = null,
                RetryCount = 0,
                MaxRetries = maxRetries
            };

            await OutboxStore.AddAsync(message);
            messageIds.Add(message.Id);
        }

        return messageIds;
    }

    protected async Task<OutboxMessage> CreateMessage(string eventType, object payload, int maxRetries = 3)
    {
        var message = new OutboxMessage
        {
            Id = Guid.NewGuid(),
            EventType = eventType,
            Payload = JsonSerializer.Serialize(payload),
            CreatedAt = DateTime.UtcNow,
            ProcessedAt = null,
            RetryCount = 0,
            MaxRetries = maxRetries
        };

        await OutboxStore.AddAsync(message);
        return message;
    }

    protected async Task<OutboxMessage> CreateTestEventMessage(int maxRetries = 3)
    {
        var testEvent = new TestDomainEvent { Data = "TestData" };
        return await CreateMessage(typeof(TestDomainEvent).AssemblyQualifiedName!, testEvent, maxRetries);
    }

    protected async Task<int> GetUnprocessedCount()
    {
        return await DbContext.OutboxMessages
            .Where(m => m.ProcessedAt == null && m.RetryCount < m.MaxRetries)
            .CountAsync();
    }

    protected async Task<int> GetProcessedCount()
    {
        return await DbContext.OutboxMessages
            .Where(m => m.ProcessedAt != null)
            .CountAsync();
    }

    protected async Task<List<DeadLetterMessage>> GetDeadLetterMessages()
    {
        return await DbContext.DeadLetterMessages.ToListAsync();
    }

    /// <summary>
    /// Runs test BackgroundServices that signal completion when target count is reached.
    /// Subscribes to ProcessingCycleCompleted events and checks count after each cycle.
    /// Uses TaskCompletionSource for signal-driven completion instead of timeouts.
    /// </summary>
    protected async Task RunTestProcessorsAsync(int workerCount, int expectedProcessedCount)
    {
        var loggerFactory = ServiceProvider.GetRequiredService<ILoggerFactory>();
        var processor = ServiceProvider.GetRequiredService<IOutboxProcessor>();

        // Completion source - signaled when target count reached
        var completionSource = new TaskCompletionSource<bool>();

        // Create and start all test workers
        var hostedServices = new List<TestOutboxBackgroundService>();
        for (int i = 1; i <= workerCount; i++)
        {
            var logger = loggerFactory.CreateLogger<TestOutboxBackgroundService>();
            var testWorker = new TestOutboxBackgroundService(
                processor,
                logger,
                workerId: i);

            // Subscribe to processing cycle events
            testWorker.ProcessingCycleCompleted += async (sender, args) =>
            {
                // Check processed count after each cycle
                var currentCount = await GetProcessedCount();

                // Signal completion if target reached
                if (currentCount >= expectedProcessedCount && !completionSource.Task.IsCompleted)
                {
                    completionSource.TrySetResult(true);
                }
            };

            hostedServices.Add(testWorker);
            await testWorker.StartAsync(CancellationToken.None);
        }

        try
        {
            // Wait for completion signal (when target count reached)
            await completionSource.Task;
        }
        finally
        {
            // Stop all workers
            foreach (var hostedService in hostedServices)
            {
                try
                {
                    await hostedService.StopAsync(CancellationToken.None);
                }
                catch
                {
                    // Ignore errors during shutdown
                }
            }

            // Small delay to ensure final operations complete
            await Task.Delay(100);
        }
    }

    /// <summary>
    /// Creates a single test BackgroundService and returns it with a completion task.
    /// Useful for tests that need to add messages dynamically while processing.
    /// Test is responsible for stopping the worker in finally block.
    /// </summary>
    /// <param name="expectedProcessedCount">The number of processed messages to wait for</param>
    /// <returns>Tuple containing the worker and a task that completes when target count is reached</returns>
    protected async Task<(TestOutboxBackgroundService Worker, Task CompletionTask)> RunTestProcessorWithCompletionAsync(int expectedProcessedCount)
    {
        var loggerFactory = ServiceProvider.GetRequiredService<ILoggerFactory>();
        var processor = ServiceProvider.GetRequiredService<IOutboxProcessor>();

        // Completion source - signaled when target count reached
        var completionSource = new TaskCompletionSource<bool>();

        // Create test worker
        var logger = loggerFactory.CreateLogger<TestOutboxBackgroundService>();
        var testWorker = new TestOutboxBackgroundService(
            processor,
            logger,
            workerId: 1);

        // Subscribe to processing cycle events
        testWorker.ProcessingCycleCompleted += async (sender, args) =>
        {
            // Check processed count after each cycle
            var currentCount = await GetProcessedCount();

            // Signal completion if target reached
            if (currentCount >= expectedProcessedCount && !completionSource.Task.IsCompleted)
            {
                completionSource.TrySetResult(true);
            }
        };

        // Start the worker
        await testWorker.StartAsync(CancellationToken.None);

        // Return worker and completion task
        return (testWorker, completionSource.Task);
    }
}

// Test handler that does nothing - used for tests
internal sealed class TestIntegrationEventHandler<TEvent> : IIntegrationEventHandler<TEvent>
    where TEvent : IDomainEvent
{
    public Task Handle(TEvent @event, CancellationToken cancellationToken = default)
    {
        return Task.CompletedTask;
    }
}

