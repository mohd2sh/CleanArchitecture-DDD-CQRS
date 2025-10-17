using CleanArchitecture.Cmms.Application.Abstractions.Common;
using CleanArchitecture.Cmms.Application.Abstractions.Messaging.Models;
using CleanArchitecture.Cmms.Application.Abstractions.Persistence.Repositories;
using CleanArchitecture.Cmms.Domain.Assets;
using CleanArchitecture.Cmms.Domain.WorkOrders.Events;

namespace CleanArchitecture.Cmms.Application.Assets.Events.WorkOrderCreatedEventHandler
{
    internal sealed class WorkOrderCreatedEventHandler
    : INotificationHandler<DomainEventNotification<WorkOrderCreatedEvent>>
    {
        private readonly IRepository<Asset, Guid> _assetRepository;
        private readonly IDateTimeProvider _dateTimeProvider;

        public WorkOrderCreatedEventHandler(
            IRepository<Asset, Guid> assetRepository, IDateTimeProvider dateTimeProvider)
        {
            _assetRepository = assetRepository;
            _dateTimeProvider = dateTimeProvider;
        }

        public async Task Handle(
            DomainEventNotification<WorkOrderCreatedEvent> notification,
            CancellationToken cancellationToken)
        {
            var domainEvent = notification.DomainEvent;

            var asset = await _assetRepository.GetByIdAsync(domainEvent.AssetId, cancellationToken);
            if (asset is null)
                return;

            asset.SetUnderMaintenance(
                description: $"Work order '{domainEvent.Title}' created",
                performedBy: "System",
                startedOn: _dateTimeProvider.UtcNow);
        }
    }
}
