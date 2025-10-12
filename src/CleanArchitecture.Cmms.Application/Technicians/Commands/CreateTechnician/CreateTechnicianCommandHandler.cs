using CleanArchitecture.Cmms.Application.Abstractions.Persistence.Repositories;
using CleanArchitecture.Cmms.Domain.Technicians;
using CleanArchitecture.Cmms.Domain.Technicians.ValueObjects;

namespace CleanArchitecture.Cmms.Application.Technicians.Commands.CreateTechnician
{
    internal sealed class CreateTechnicianCommandHandler
     : ICommandHandler<CreateTechnicianCommand, Result<Guid>>
    {
        private readonly IRepository<Technician, Guid> _repository;

        public CreateTechnicianCommandHandler(IRepository<Technician, Guid> repository)
        {
            _repository = repository;
        }

        public async Task<Result<Guid>> Handle(CreateTechnicianCommand request, CancellationToken cancellationToken)
        {
            var skillLevel = new SkillLevel(request.SkillLevelName, request.SkillLevelRank);

            var technician = Technician.Create(request.Name, skillLevel);

            await _repository.AddAsync(technician, cancellationToken);

            return technician.Id;
        }
    }
}
