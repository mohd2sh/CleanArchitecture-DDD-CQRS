using CleanArchitecture.Cmms.Application.Abstractions.Persistence.Repositories;
using CleanArchitecture.Cmms.Domain.Technicians;

namespace CleanArchitecture.Cmms.Application.Technicians.Commands.CompleteAssignment
{
    internal sealed class CompleteAssignmentCommandHandler
      : ICommandHandler<CompleteAssignmentCommand, Result>
    {
        private readonly IRepository<Technician, Guid> _repository;

        public CompleteAssignmentCommandHandler(IRepository<Technician, Guid> repository)
        {
            _repository = repository;
        }

        public async Task<Result> Handle(CompleteAssignmentCommand request, CancellationToken cancellationToken)
        {
            var technician = await _repository.GetByIdAsync(request.TechnicianId, cancellationToken);

            if (technician is null)
                return $"Technician {request.TechnicianId} not found.";


            technician.CompleteAssignment(request.WorkOrderId, request.CompletedOn);

            await _repository.UpdateAsync(technician, cancellationToken);

            return Result.Success();
        }
    }
}
