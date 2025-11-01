using CleanArchitecture.Cmms.Domain.WorkOrders.Events;
using CleanArchitecture.Core.Application.Abstractions.Events;
using Microsoft.Extensions.Logging;

namespace CleanArchitecture.Cmms.Application.WorkOrders.Events.WorkOrderCreated;

public class WorkOrderCreatedEventHandler : IDomainEventHandler<WorkOrderCreatedEvent>
{
    private readonly ILogger<WorkOrderCreatedEventHandler> _logger;

    public WorkOrderCreatedEventHandler(ILogger<WorkOrderCreatedEventHandler> logger)
    {
        _logger = logger;
    }

    public Task Handle(WorkOrderCreatedEvent domainEvent, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Work Order Created Event Handled: {WorkOrderId}", domainEvent.WorkOrderId);

        return Task.CompletedTask;
    }
}
