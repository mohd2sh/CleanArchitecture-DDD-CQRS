using CleanArchitecture.Cmms.Application.Abstractions.Events;
using CleanArchitecture.Cmms.Domain.WorkOrders.Events;
using Microsoft.Extensions.Logging;

namespace CleanArchitecture.Cmms.Application.WorkOrders.EventsHandlers
{
    public class WorkOrderCreatedEventHandler : IDomainEventHandler<WorkOrderCreatedEvent>
    {
        private readonly ILogger<WorkOrderCreatedEventHandler> _logger;

        public WorkOrderCreatedEventHandler(ILogger<WorkOrderCreatedEventHandler> logger)
        {
            _logger = logger;
        }

        public Task Handle(WorkOrderCreatedEvent domainEvent, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Work Order Created Event Handled: {WorkOrderId}", domainEvent.WorkOrderId);

            return Task.CompletedTask;
        }
    }
}
