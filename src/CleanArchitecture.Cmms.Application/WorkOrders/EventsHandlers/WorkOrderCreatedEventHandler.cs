using CleanArchitecture.Cmms.Application.Abstractions.Messaging.Models;
using CleanArchitecture.Cmms.Domain.WorkOrders.Events;
using Microsoft.Extensions.Logging;

namespace CleanArchitecture.Cmms.Application.WorkOrders.EventsHandlers
{
    public class WorkOrderCreatedEventHandler : INotificationHandler<DomainEventNotification<WorkOrderCreatedEvent>>
    {
        private readonly ILogger<WorkOrderCreatedEventHandler> _logger;

        public WorkOrderCreatedEventHandler(ILogger<WorkOrderCreatedEventHandler> logger)
        {
            _logger = logger;
        }

        public Task Handle(DomainEventNotification<WorkOrderCreatedEvent> notification, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Work Order Created Event Handled: {WorkOrderId}", notification.DomainEvent.WorkOrderId);

            return Task.CompletedTask;
        }
    }
}
