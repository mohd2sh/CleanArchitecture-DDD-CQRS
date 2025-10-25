using CleanArchitecture.Cmms.Application.Abstractions.Common;
using CleanArchitecture.Cmms.Application.Abstractions.Persistence.Repositories;
using CleanArchitecture.Cmms.Domain.WorkOrders;

namespace CleanArchitecture.Cmms.Application.WorkOrders.Commands.AssignTechnician
{

    internal sealed class AssignTechnicianCommandHandler
    : ICommandHandler<AssignTechnicianCommand, Result>
    {
        private readonly IRepository<WorkOrder, Guid> _workOrderRepository;

        public AssignTechnicianCommandHandler(
        IRepository<WorkOrder, Guid> workOrderRepository)
        {
            _workOrderRepository = workOrderRepository;
        }

        public async Task<Result> Handle(AssignTechnicianCommand request, CancellationToken cancellationToken = default)
        {
            var workOrder = await _workOrderRepository.GetByIdAsync(request.WorkOrderId, cancellationToken);

            if (workOrder is null)
                return Application.WorkOrders.WorkOrderErrors.NotFound;

            workOrder.AssignTechnician(request.TechnicianId);

            await _workOrderRepository.UpdateAsync(workOrder, cancellationToken);

            return Result.Success();
        }
    }
}
