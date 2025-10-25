using CleanArchitecture.Core.Application.Abstractions.Common;
using CleanArchitecture.Core.Application.Abstractions.Persistence.Repositories;
using CleanArchitecture.Cmms.Domain.WorkOrders;

namespace CleanArchitecture.Cmms.Application.WorkOrders.Commands.StartWorkOrder
{
    internal sealed class StartWorkOrderCommandHandler : ICommandHandler<StartWorkOrderCommand, Result>
    {
        private readonly IRepository<WorkOrder, Guid> _repository;

        public StartWorkOrderCommandHandler(IRepository<WorkOrder, Guid> repository)
        {
            _repository = repository;
        }

        public async Task<Result> Handle(StartWorkOrderCommand request, CancellationToken ct)
        {
            var workOrder = await _repository.GetByIdAsync(request.WorkOrderId, ct);

            if (workOrder is null)
                return WorkOrderErrors.NotFound;

            workOrder.Start();

            await _repository.UpdateAsync(workOrder, ct);

            return Result.Success();
        }
    }

}
