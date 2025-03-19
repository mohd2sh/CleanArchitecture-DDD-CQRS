using CleanArchitecture.Cmms.Application.Abstractions.Messaging;
using CleanArchitecture.Cmms.Application.Abstractions.Messaging.Models;
using CleanArchitecture.Cmms.Domain.WorkOrders.Events;

namespace CleanArchitecture.Cmms.Application.WorkOrders.EventsHandlers
{
    public class WorkOrderCreatedEventHandler : INotificationHandler<DomainEventNotification<WorkOrderCreatedEvent>>
    {
        public async Task Handle(DomainEventNotification<WorkOrderCreatedEvent> notification, CancellationToken cancellationToken)
        {

            Console.WriteLine($"Work Order Created Event Handled: {notification.DomainEvent.WorkOrderId}");
        }
    }
}
