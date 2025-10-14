using CleanArchitecture.Cmms.Application.Abstractions.Messaging.Models;
using CleanArchitecture.Cmms.Application.Abstractions.Persistence.Repositories;
using CleanArchitecture.Cmms.Domain.Assets;
using CleanArchitecture.Cmms.Domain.WorkOrders.Events;
using Microsoft.Extensions.Logging;

namespace CleanArchitecture.Cmms.Application.Assets.Events.WorkOrderCompleted
{
    internal sealed class WorkOrderCompletedEventHandler
    : INotificationHandler<DomainEventNotification<WorkOrderCompletedEvent>>
    {
        private readonly IRepository<Asset, Guid> _assetRepository;
        private readonly ILogger<WorkOrderCompletedEventHandler> _logger;

        public WorkOrderCompletedEventHandler(IRepository<Asset, Guid> assetRepository, ILogger<WorkOrderCompletedEventHandler> logger)
        {
            _assetRepository = assetRepository;
            _logger = logger;
        }

        public async Task Handle(
            DomainEventNotification<WorkOrderCompletedEvent> notification,
            CancellationToken cancellationToken)
        {
            var domainEvent = notification.DomainEvent;

            var asset = await _assetRepository.GetByIdAsync(domainEvent.AssetId, cancellationToken);
            if (asset is null)
            {
                _logger.LogWarning("Asset with ID {AssetId} not found for Work Order {WorkOrderId}",
                    domainEvent.AssetId, domainEvent.WorkOrderId);
                return;
            }

            //TODO: IDatetimeProvider
            asset.CompleteMaintenance(DateTime.UtcNow, "Work order completed successfully");
        }
    }
}
