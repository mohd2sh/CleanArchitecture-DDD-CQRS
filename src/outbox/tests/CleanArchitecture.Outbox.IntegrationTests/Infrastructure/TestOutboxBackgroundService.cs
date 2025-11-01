using CleanArchitecture.Outbox.Processing;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace CleanArchitecture.Outbox.IntegrationTests.Infrastructure;

public sealed class TestOutboxBackgroundService : BackgroundService
{
    private readonly IOutboxProcessor _processor;
    private readonly ILogger<TestOutboxBackgroundService> _logger;
    private readonly int _workerId;
    private readonly TimeSpan _idleDelay = TimeSpan.FromMilliseconds(100);

    /// <summary>
    /// Event raised after each processing cycle.
    /// </summary>
    public event EventHandler ProcessingCycleCompleted;

    public TestOutboxBackgroundService(
        IOutboxProcessor processor,
        ILogger<TestOutboxBackgroundService> logger,
        int workerId)
    {
        _processor = processor;
        _logger = logger;
        _workerId = workerId;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Test outbox processor worker {WorkerId} started", _workerId);

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await _processor.ProcessOutboxMessagesAsync(stoppingToken);

                // Raise event after each cycle - test decides when to stop
                ProcessingCycleCompleted?.Invoke(this, EventArgs.Empty);

                // Small delay to avoid tight loop when no messages found
                await Task.Delay(_idleDelay, stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing outbox messages in test worker {WorkerId}", _workerId);
            }
        }

        _logger.LogInformation("Test outbox processor worker {WorkerId} stopped", _workerId);
    }
}

