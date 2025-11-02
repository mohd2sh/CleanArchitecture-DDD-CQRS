using CleanArchitecture.Cmms.Domain.Assets;
using CleanArchitecture.Cmms.Domain.WorkOrders.Events;
using CleanArchitecture.Core.Application.Abstractions.Common;
using CleanArchitecture.Core.Application.Abstractions.Events;
using CleanArchitecture.Core.Application.Abstractions.Persistence.Repositories;

namespace CleanArchitecture.Cmms.Application.Assets.Events.WorkOrderCreatedEventHandler;

internal sealed class WorkOrderCreatedEventHandler
: IDomainEventHandler<WorkOrderCreatedEvent>
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
        WorkOrderCreatedEvent domainEvent,
        CancellationToken cancellationToken = default)
    {
        var asset = await _assetRepository.GetByIdAsync(domainEvent.AssetId, cancellationToken);

        if (asset is null)
        {
            throw new Core.Application.Abstractions.Common.ApplicationException(AssetErrors.NotFound);
        }

        asset.SetUnderMaintenance(
            description: $"Work order '{domainEvent.Title}' created",
            performedBy: "System",
            startedOn: _dateTimeProvider.UtcNow);
    }
}
