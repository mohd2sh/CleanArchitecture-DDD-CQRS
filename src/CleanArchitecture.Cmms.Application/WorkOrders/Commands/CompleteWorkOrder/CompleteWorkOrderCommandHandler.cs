using CleanArchitecture.Cmms.Domain.WorkOrders;
using CleanArchitecture.Core.Application.Abstractions.Common;
using CleanArchitecture.Core.Application.Abstractions.Persistence.Repositories;

namespace CleanArchitecture.Cmms.Application.WorkOrders.Commands.CompleteWorkOrder
{
    internal sealed class CompleteWorkOrderCommandHandler
     : ICommandHandler<CompleteWorkOrderCommand, Result>
    {
        private readonly IRepository<WorkOrder, Guid> _workOrderRepository;

        public CompleteWorkOrderCommandHandler(
            IRepository<WorkOrder, Guid> workOrderRepository)
        {
            _workOrderRepository = workOrderRepository;
        }

        public async Task<Result> Handle(CompleteWorkOrderCommand request, CancellationToken cancellationToken)
        {
            var workOrder = await _workOrderRepository.GetByIdAsync(request.WorkOrderId, cancellationToken);

            if (workOrder is null)
                return Application.WorkOrders.WorkOrderErrors.NotFound;

            workOrder.Complete();

            await _workOrderRepository.UpdateAsync(workOrder, cancellationToken);

            return Result.Success();
        }
    }
}
