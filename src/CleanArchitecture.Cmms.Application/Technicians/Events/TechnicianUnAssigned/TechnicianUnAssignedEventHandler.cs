using CleanArchitecture.Cmms.Domain.Technicians;
using CleanArchitecture.Cmms.Domain.WorkOrders.Events;
using CleanArchitecture.Core.Application.Abstractions.Events;
using CleanArchitecture.Core.Application.Abstractions.Persistence.Repositories;

namespace CleanArchitecture.Cmms.Application.Technicians.Events.TechnicianUnAssigned
{
    internal class TechnicianUnAssignedEventHandler : IDomainEventHandler<TechnicianUnAssignedEvent>
    {
        private readonly IRepository<Technician, Guid> _technicianRepository;

        public TechnicianUnAssignedEventHandler(IRepository<Technician, Guid> technicianRepository)
        {
            _technicianRepository = technicianRepository;
        }

        public async Task Handle(TechnicianUnAssignedEvent domainEvent, CancellationToken cancellationToken = default)
        {
            var technician = await _technicianRepository.GetByIdAsync(domainEvent.TechnicianId, cancellationToken);

            if (technician == null)
                throw new Core.Application.Abstractions.Common.ApplicationException(TechnicianErrors.NotFound);

            technician.UnAssignedOrder(domainEvent.WorkOrderId);

            await _technicianRepository.UpdateAsync(technician, cancellationToken);
        }
    }
}
