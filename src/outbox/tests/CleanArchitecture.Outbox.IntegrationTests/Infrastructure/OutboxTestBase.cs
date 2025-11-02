using System.Text.Json;
using CleanArchitecture.Core.Application.Abstractions.Events;
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

    private static readonly TimeSpan DefaultProcessingTimeout = TimeSpan.FromSeconds(60);

    private (ILoggerFactory LoggerFactory, IOutboxProcessor Processor) GetRequiredServices()
    {
        var loggerFactory = ServiceProvider.GetRequiredService<ILoggerFactory>();
        var processor = ServiceProvider.GetRequiredService<IOutboxProcessor>();
        return (loggerFactory, processor);
    }

    private EventHandler CreateCompletionHandler(TaskCompletionSource<bool> completionSource, int expectedProcessedCount)
    {
        return async (sender, args) =>
        {
            using var scope = _factory.Services.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<OutboxDbContext>();

            var currentCount = await dbContext.OutboxMessages
                .Where(m => m.ProcessedAt != null)
                .CountAsync();

            if (currentCount >= expectedProcessedCount && !completionSource.Task.IsCompleted)
            {
                completionSource.TrySetResult(true);
            }
        };
    }

    private static async Task<TestOutboxBackgroundService> CreateAndSubscribeWorker(
        IOutboxProcessor processor,
        ILoggerFactory loggerFactory,
        int workerId,
        EventHandler handler)
    {
        var logger = loggerFactory.CreateLogger<TestOutboxBackgroundService>();
        var worker = new TestOutboxBackgroundService(processor, logger, workerId);
        worker.ProcessingCycleCompleted += handler;
        await worker.StartAsync(CancellationToken.None);
        return worker;
    }

    private async Task WaitForCompletionOrTimeout(
        TaskCompletionSource<bool> completionSource,
        int expectedProcessedCount,
        TimeSpan timeout)
    {
        var timeoutTask = Task.Delay(timeout);
        var completedTask = await Task.WhenAny(completionSource.Task, timeoutTask);

        if (completedTask == timeoutTask)
        {
            var currentProcessed = await GetProcessedCount();
            var currentUnprocessed = await GetUnprocessedCount();
            throw new TimeoutException(
                $"Timeout waiting for {expectedProcessedCount} messages to be processed. " +
                $"Current state: {currentProcessed} processed, {currentUnprocessed} unprocessed.");
        }

        await completionSource.Task;
    }

    private Task<bool> CreateCompletionTaskWithTimeout(
        TaskCompletionSource<bool> completionSource,
        int expectedProcessedCount,
        TimeSpan timeout)
    {
        var timeoutTask = Task.Delay(timeout).ContinueWith(async _ =>
        {
            if (!completionSource.Task.IsCompleted)
            {
                var currentProcessed = await GetProcessedCount();
                throw new TimeoutException(
                    $"Timeout waiting for {expectedProcessedCount} messages to be processed. " +
                    $"Current state: {currentProcessed} processed.");
            }
        }, TaskScheduler.Default).Unwrap();

        return Task.WhenAny(completionSource.Task, timeoutTask)
            .ContinueWith(async t =>
            {
                if (t.Result == timeoutTask)
                {
                    await timeoutTask;
                    return false;
                }
                await completionSource.Task;
                return true;
            }, TaskScheduler.Default).Unwrap();
    }

    private static async Task StopWorkers(IEnumerable<TestOutboxBackgroundService> workers)
    {
        foreach (var worker in workers)
        {
            try
            {
                await worker.StopAsync(CancellationToken.None);
            }
            catch
            {
                // Ignore errors during shutdown
            }
        }

        await Task.Delay(100);
    }

    /// <summary>
    /// Runs test BackgroundServices that signal completion when target count is reached.
    /// Subscribes to ProcessingCycleCompleted events and checks count after each cycle.
    /// Uses TaskCompletionSource for signal-driven completion with configurable timeout.
    /// </summary>
    /// <param name="workerCount">Number of worker threads to start</param>
    /// <param name="expectedProcessedCount">Expected number of messages to process</param>
    /// <param name="timeout">Timeout for waiting (default: 60 seconds)</param>
    protected async Task RunTestProcessorsAsync(int workerCount, int expectedProcessedCount, TimeSpan? timeout = null)
    {
        var (loggerFactory, processor) = GetRequiredServices();
        var completionSource = new TaskCompletionSource<bool>();
        var handler = CreateCompletionHandler(completionSource, expectedProcessedCount);

        var workers = new List<TestOutboxBackgroundService>();
        for (int i = 1; i <= workerCount; i++)
        {
            var worker = await CreateAndSubscribeWorker(processor, loggerFactory, i, handler);
            workers.Add(worker);
        }

        try
        {
            var timeoutValue = timeout ?? DefaultProcessingTimeout;
            await WaitForCompletionOrTimeout(completionSource, expectedProcessedCount, timeoutValue);
        }
        finally
        {
            await StopWorkers(workers);
        }
    }

    /// <summary>
    /// Creates a single test BackgroundService and returns it with a completion task.
    /// Useful for tests that need to add messages dynamically while processing.
    /// Test is responsible for stopping the worker in finally block.
    /// </summary>
    /// <param name="expectedProcessedCount">The number of processed messages to wait for</param>
    /// <param name="timeout">Timeout for waiting (default: 60 seconds)</param>
    /// <returns>Tuple containing the worker and a task that completes when target count is reached</returns>
    protected async Task<(TestOutboxBackgroundService Worker, Task<bool> CompletionTask)> RunTestProcessorWithCompletionAsync(int expectedProcessedCount, TimeSpan? timeout = null)
    {
        var (loggerFactory, processor) = GetRequiredServices();
        var completionSource = new TaskCompletionSource<bool>();
        var handler = CreateCompletionHandler(completionSource, expectedProcessedCount);

        var worker = await CreateAndSubscribeWorker(processor, loggerFactory, workerId: 1, handler);

        var timeoutValue = timeout ?? DefaultProcessingTimeout;
        var completionTask = CreateCompletionTaskWithTimeout(completionSource, expectedProcessedCount, timeoutValue);

        return (worker, completionTask);
    }
}

// Test handler that does nothing - used for tests
internal sealed class TestIntegrationEventHandler<TEvent> : IIntegrationEventHandler<TEvent>
{
    public Task Handle(TEvent @event, CancellationToken cancellationToken = default)
    {
        return Task.CompletedTask;
    }
}

