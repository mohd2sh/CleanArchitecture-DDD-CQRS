using CleanArchitecture.Cmms.Domain.WorkOrders;
using CleanArchitecture.Cmms.Domain.WorkOrders.ValueObjects;
using CleanArchitecture.Core.Application.Abstractions.Common;
using CleanArchitecture.Core.Application.Abstractions.Persistence.Repositories;

namespace CleanArchitecture.Cmms.Application.WorkOrders.Commands.CreateWorkOrder
{
    internal sealed class CreateWorkOrderCommandHandler : ICommandHandler<CreateWorkOrderCommand, Result<Guid>>
    {
        private readonly IRepository<WorkOrder, Guid> _workOrderRepository;

        public CreateWorkOrderCommandHandler(IRepository<WorkOrder, Guid> workOrderRepository)
        {
            _workOrderRepository = workOrderRepository;
        }

        public async Task<Result<Guid>> Handle(CreateWorkOrderCommand request, CancellationToken cancellationToken = default)
        {
            var location = Location.Create(request.Building, request.Floor, request.Room);

            var workOrder = WorkOrder.Create(request.AssetId, request.Title, location);

            await _workOrderRepository.AddAsync(workOrder, cancellationToken);

            return workOrder.Id;
        }
    }
}
