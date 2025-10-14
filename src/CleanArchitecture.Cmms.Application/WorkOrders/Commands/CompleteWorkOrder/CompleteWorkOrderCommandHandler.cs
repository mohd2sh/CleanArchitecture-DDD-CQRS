using CleanArchitecture.Cmms.Application.Abstractions.Persistence.Repositories;
using CleanArchitecture.Cmms.Domain.Technicians;
using CleanArchitecture.Cmms.Domain.WorkOrders;

namespace CleanArchitecture.Cmms.Application.WorkOrders.Commands.CompleteWorkOrder
{
    internal sealed class CompleteWorkOrderCommandHandler
     : ICommandHandler<CompleteWorkOrderCommand, Result>
    {
        private readonly IRepository<WorkOrder, Guid> _workOrderRepository;
        private readonly IRepository<Technician, Guid> _technicianRepository;

        public CompleteWorkOrderCommandHandler(
            IRepository<WorkOrder, Guid> workOrderRepository,
            IRepository<Technician, Guid> technicianRepository)
        {
            _workOrderRepository = workOrderRepository;
            _technicianRepository = technicianRepository;
        }

        public async Task<Result> Handle(CompleteWorkOrderCommand request, CancellationToken cancellationToken)
        {
            var workOrder = await _workOrderRepository.GetByIdAsync(request.WorkOrderId, cancellationToken);

            if (workOrder is null)
                return "Work order not found.";

            if (workOrder.TechnicianId is null)
                return "Cannot complete a work order without an assigned technician.";

            var technician = await _technicianRepository.GetByIdAsync(workOrder.TechnicianId.Value, cancellationToken);

            if (technician is null)
                return "Cannot complete a work order without an a technician";

            workOrder.Complete();

            await _workOrderRepository.UpdateAsync(workOrder, cancellationToken);

            technician.CompleteAssignment(workOrder.Id, DateTime.UtcNow);//ToDO: IDateTimeProvider

            await _technicianRepository.UpdateAsync(technician, cancellationToken);

            return Result.Success();
        }
    }
}
