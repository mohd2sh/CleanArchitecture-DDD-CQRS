using CleanArchitecture.Cmms.Domain.Assets;
using CleanArchitecture.Cmms.Domain.WorkOrders.Events;
using CleanArchitecture.Core.Application.Abstractions.Common;
using CleanArchitecture.Core.Application.Abstractions.Events;
using CleanArchitecture.Core.Application.Abstractions.Persistence.Repositories;
using Microsoft.Extensions.Logging;

namespace CleanArchitecture.Cmms.Application.Assets.Events.WorkOrderCompleted;

internal sealed class WorkOrderCompletedEventHandler
: IDomainEventHandler<WorkOrderCompletedEvent>
{
    private readonly IRepository<Asset, Guid> _assetRepository;
    private readonly IDateTimeProvider _dateTimeProvider;
    private readonly ILogger<WorkOrderCompletedEventHandler> _logger;

    public WorkOrderCompletedEventHandler(IRepository<Asset, Guid> assetRepository, IDateTimeProvider dateTimeProvider, ILogger<WorkOrderCompletedEventHandler> logger)
    {
        _assetRepository = assetRepository;
        _dateTimeProvider = dateTimeProvider;
        _logger = logger;
    }

    public async Task Handle(
        WorkOrderCompletedEvent domainEvent,
        CancellationToken cancellationToken = default)
    {
        var asset = await _assetRepository.GetByIdAsync(domainEvent.AssetId, cancellationToken);
        if (asset is null)
        {
            _logger.LogWarning("Asset with ID {AssetId} not found for Work Order {WorkOrderId}",
                domainEvent.AssetId, domainEvent.WorkOrderId);

            throw new Core.Application.Abstractions.Common.ApplicationException(AssetErrors.NotFound);
        }

        asset.CompleteMaintenance(_dateTimeProvider.UtcNow, "Work order completed successfully");
    }
}
