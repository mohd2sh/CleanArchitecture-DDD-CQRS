using CleanArchitecture.Cmms.Domain.Technicians;
using CleanArchitecture.Cmms.Domain.WorkOrders.Events;
using CleanArchitecture.Core.Application.Abstractions.Events;
using CleanArchitecture.Core.Application.Abstractions.Persistence.Repositories;

namespace CleanArchitecture.Cmms.Application.Technicians.Events.WorkOrderCompleted
{
    internal sealed class WorkOrderCompletedEventHandler : IDomainEventHandler<WorkOrderCompletedEvent>
    {
        private readonly IRepository<Technician, Guid> _technicianRepository;

        public WorkOrderCompletedEventHandler(IRepository<Technician, Guid> technicianRepository)
        {
            _technicianRepository = technicianRepository;
        }

        public async Task Handle(WorkOrderCompletedEvent domainEvent, CancellationToken cancellationToken = default)
        {
            var technician = await _technicianRepository.GetByIdAsync(domainEvent.TechnicianId, cancellationToken);

            if (technician is null)
            {
                throw new Core.Application.Abstractions.Common.ApplicationException(TechnicianErrors.NotFound);
            }

            technician.CompleteAssignment(domainEvent.WorkOrderId, domainEvent.OccurredOn ?? DateTime.UtcNow);
        }
    }
}
