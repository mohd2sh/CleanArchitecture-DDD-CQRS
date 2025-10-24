using CleanArchitecture.Cmms.Application.Abstractions.Events;
using CleanArchitecture.Cmms.Domain.WorkOrders.Events;
using Microsoft.Extensions.Logging;

namespace CleanArchitecture.Cmms.Application.Integrations.Events.WorkOrderCompleted;

/// <summary>
/// Integration event handler 
/// This handler executes asynchronously via outbox pattern for guaranteed delivery.
/// Its not doing anything currently just showing the pattern.
/// </summary>
internal sealed class EmailWorkOrderCompletedHandler
    : IIntegrationEventHandler<WorkOrderCompletedEvent>
{
    private readonly IEmailService _emailService;
    private readonly ILogger<EmailWorkOrderCompletedHandler> _logger;

    public EmailWorkOrderCompletedHandler(
        IEmailService emailService,
        ILogger<EmailWorkOrderCompletedHandler> logger)
    {
        _emailService = emailService;
        _logger = logger;
    }

    public async Task Handle(WorkOrderCompletedEvent @event, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Sending completion email for work order {WorkOrderId}", @event.WorkOrderId);

        try
        {
            await _emailService.SendWorkOrderCompletedEmail(@event.WorkOrderId, @event.AssetId);

            _logger.LogInformation("Completion email sent for work order {WorkOrderId}", @event.WorkOrderId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send completion email for work order {WorkOrderId}", @event.WorkOrderId);
            throw; // Let outbox processor handle retry logic
        }
    }
}

/// <summary>
/// Mock email service for demonstration purposes.
/// In a real application, this would integrate with email providers like SendGrid, AWS SES, etc.
/// </summary>
public interface IEmailService
{
    Task SendWorkOrderCompletedEmail(Guid workOrderId, Guid assetId);
}

internal sealed class MockEmailService : IEmailService
{
    private readonly ILogger<MockEmailService> _logger;

    public MockEmailService(ILogger<MockEmailService> logger)
    {
        _logger = logger;
    }

    public async Task SendWorkOrderCompletedEmail(Guid workOrderId, Guid assetId)
    {
        _logger.LogInformation("Mock: Sending email for work order {WorkOrderId} on asset {AssetId}", workOrderId, assetId);

        // Simulate email sending delay
        await Task.Delay(100);

        _logger.LogInformation("Mock: Email sent successfully for work order {WorkOrderId}", workOrderId);
    }
}
