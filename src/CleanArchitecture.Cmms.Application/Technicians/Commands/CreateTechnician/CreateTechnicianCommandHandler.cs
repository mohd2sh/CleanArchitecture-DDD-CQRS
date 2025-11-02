using CleanArchitecture.Cmms.Domain.Technicians;
using CleanArchitecture.Cmms.Domain.Technicians.ValueObjects;
using CleanArchitecture.Core.Application.Abstractions.Common;
using CleanArchitecture.Core.Application.Abstractions.Persistence.Repositories;

namespace CleanArchitecture.Cmms.Application.Technicians.Commands.CreateTechnician;

internal sealed class CreateTechnicianCommandHandler
 : ICommandHandler<CreateTechnicianCommand, Result<Guid>>
{
    private readonly IRepository<Technician, Guid> _repository;

    public CreateTechnicianCommandHandler(IRepository<Technician, Guid> repository)
    {
        _repository = repository;
    }

    public async Task<Result<Guid>> Handle(CreateTechnicianCommand request, CancellationToken cancellationToken = default)
    {
        var skillLevel = SkillLevel.Create(request.SkillLevelName, request.SkillLevelRank);

        var technician = Technician.Create(request.Name, skillLevel);

        await _repository.AddAsync(technician, cancellationToken);

        return technician.Id;
    }
}
