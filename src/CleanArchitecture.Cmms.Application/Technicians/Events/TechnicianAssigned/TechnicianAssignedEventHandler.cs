using CleanArchitecture.Cmms.Domain.Technicians;
using CleanArchitecture.Cmms.Domain.WorkOrders.Events;
using CleanArchitecture.Core.Application.Abstractions.Events;
using CleanArchitecture.Core.Application.Abstractions.Persistence.Repositories;

namespace CleanArchitecture.Cmms.Application.Technicians.Events.TechnicianAssigned;

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
            throw new Core.Application.Abstractions.Common.ApplicationException(TechnicianErrors.NotFound);

        technician.AddAssignedOrder(domainEvent.WorkOrderId, domainEvent.OccurredOn ?? DateTime.UtcNow);

        await _technicianRepository.UpdateAsync(technician, cancellationToken);
    }
}
