using CleanArchitecture.Cmms.Application.Abstractions.Messaging;
using CleanArchitecture.Cmms.Application.Abstractions.Persistence.Repositories;
using CleanArchitecture.Cmms.Application.Primitives;
using CleanArchitecture.Cmms.Domain.Technicians;
using CleanArchitecture.Cmms.Domain.WorkOrders;

namespace CleanArchitecture.Cmms.Application.WorkOrders.Commands.AssignTechnician
{
    internal sealed class AssignTechnicianCommandHandler
    : ICommandHandler<AssignTechnicianCommand, Result>
    {
        private readonly IRepository<WorkOrder, Guid> _workOrderRepository;
        private readonly IRepository<Technician, Guid> _technicianRepository;
        public AssignTechnicianCommandHandler(
        IRepository<WorkOrder, Guid> workOrderRepository,
        IRepository<Technician, Guid> technicianRepository)
        {
            _workOrderRepository = workOrderRepository;
            _technicianRepository = technicianRepository;
        }

        public async Task<Result> Handle(AssignTechnicianCommand request, CancellationToken ct)
        {
            var technician = await _technicianRepository.GetByIdAsync(request.TechnicianId, ct);
            if (technician is null)
                return "Technician not found.";

            var workOrder = await _workOrderRepository.GetByIdAsync(request.WorkOrderId, ct);
            if (workOrder is null)
                return "Work order not found.";

            technician.AddAssignedOrder(workOrder.Id, DateTime.UtcNow);//TODO: IDateTimeProvider

            workOrder.AssignTechnician(technician.Id);

            await _technicianRepository.UpdateAsync(technician, ct);

            await _workOrderRepository.UpdateAsync(workOrder, ct);

            return Result.Success();
        }
    }
}
