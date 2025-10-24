using CleanArchitecture.Cmms.Application.Abstractions.Events;
using CleanArchitecture.Cmms.Application.Abstractions.Persistence.Repositories;
using CleanArchitecture.Cmms.Domain.Technicians;
using CleanArchitecture.Cmms.Domain.WorkOrders.Events;

namespace CleanArchitecture.Cmms.Application.Technicians.Events.TechnicianAssignedEventHandler
{
    internal class TechnicianAssignedEventHandler : IDomainEventHandler<TechnicianAssignedEvent>
    {
        private readonly IRepository<Technician, Guid> _technicianRepository;

        public TechnicianAssignedEventHandler(IRepository<Technician, Guid> technicianRepository)
        {
            _technicianRepository = technicianRepository;
        }

        public async Task Handle(TechnicianAssignedEvent domainEvent, CancellationToken cancellationToken = default)
        {
            var technician = await _technicianRepository.GetByIdAsync(domainEvent.TechnicianId, cancellationToken);

            if (technician == null)
                throw new Abstractions.Common.ApplicationException(TechnicianErrors.NotFound);

            technician.AddAssignedOrder(domainEvent.WorkOrderId, domainEvent.OccurredOn.Value);

            await _technicianRepository.UpdateAsync(technician, cancellationToken);
        }
    }
}
