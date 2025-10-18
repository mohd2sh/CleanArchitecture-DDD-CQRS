using CleanArchitecture.Cmms.Application.Abstractions.Common;
using CleanArchitecture.Cmms.Application.Abstractions.Persistence.Repositories;
using CleanArchitecture.Cmms.Application.Technicians;
using CleanArchitecture.Cmms.Application.WorkOrders;
using CleanArchitecture.Cmms.Domain.Technicians;
using CleanArchitecture.Cmms.Domain.WorkOrders;

namespace CleanArchitecture.Cmms.Application.WorkOrders.Commands.CompleteWorkOrder
{
    internal sealed class CompleteWorkOrderCommandHandler
     : ICommandHandler<CompleteWorkOrderCommand, Result>
    {
        private readonly IRepository<WorkOrder, Guid> _workOrderRepository;
        private readonly IRepository<Technician, Guid> _technicianRepository;
        private readonly IDateTimeProvider _dateTimeProvider;

        public CompleteWorkOrderCommandHandler(
            IRepository<WorkOrder, Guid> workOrderRepository,
            IRepository<Technician, Guid> technicianRepository,
            IDateTimeProvider dateTimeProvider)
        {
            _workOrderRepository = workOrderRepository;
            _technicianRepository = technicianRepository;
            _dateTimeProvider = dateTimeProvider;
        }

        public async Task<Result> Handle(CompleteWorkOrderCommand request, CancellationToken cancellationToken)
        {
            var workOrder = await _workOrderRepository.GetByIdAsync(request.WorkOrderId, cancellationToken);

            if (workOrder is null)
                return Application.WorkOrders.WorkOrderErrors.NotFound;

            if (workOrder.TechnicianId is null)
                return Application.WorkOrders.WorkOrderErrors.TechnicianRequired;

            var technician = await _technicianRepository.GetByIdAsync(workOrder.TechnicianId.Value, cancellationToken);

            if (technician is null)
                return Application.Technicians.TechnicianErrors.NotFound;

            workOrder.Complete();

            await _workOrderRepository.UpdateAsync(workOrder, cancellationToken);

            technician.CompleteAssignment(workOrder.Id, _dateTimeProvider.UtcNow);

            await _technicianRepository.UpdateAsync(technician, cancellationToken);

            return Result.Success();
        }
    }
}
