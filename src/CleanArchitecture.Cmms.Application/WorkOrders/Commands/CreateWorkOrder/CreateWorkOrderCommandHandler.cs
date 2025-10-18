using CleanArchitecture.Cmms.Application.Abstractions.Persistence.Repositories;
using CleanArchitecture.Cmms.Application.Assets;
using CleanArchitecture.Cmms.Domain.Assets;
using CleanArchitecture.Cmms.Domain.WorkOrders;
using CleanArchitecture.Cmms.Domain.WorkOrders.ValueObjects;

namespace CleanArchitecture.Cmms.Application.WorkOrders.Commands.CreateWorkOrder
{
    internal sealed class CreateWorkOrderCommandHandler : ICommandHandler<CreateWorkOrderCommand, Result<Guid>>
    {
        private readonly IRepository<WorkOrder, Guid> _workOrderRepository;
        private readonly IRepository<Asset, Guid> _assetRepository;

        public CreateWorkOrderCommandHandler(IRepository<WorkOrder, Guid> workOrderRepository, IRepository<Asset, Guid> assetRepository)
        {
            _workOrderRepository = workOrderRepository;
            _assetRepository = assetRepository;
        }

        public async Task<Result<Guid>> Handle(CreateWorkOrderCommand request, CancellationToken cancellationToken)
        {
            var asset = await _assetRepository.GetByIdAsync(request.AssetId, cancellationToken);

            if (asset is null)
                return Application.Assets.AssetErrors.NotFound;

            if (!asset.IsAvailable())
            {
                return Application.Assets.AssetErrors.NotAvailable;
            }

            var location = Location.Create(request.Building, request.Floor, request.Room);

            var workOrder = WorkOrder.Create(request.AssetId, request.Title, location);

            await _workOrderRepository.AddAsync(workOrder, cancellationToken);

            return workOrder.Id;
        }
    }
}
