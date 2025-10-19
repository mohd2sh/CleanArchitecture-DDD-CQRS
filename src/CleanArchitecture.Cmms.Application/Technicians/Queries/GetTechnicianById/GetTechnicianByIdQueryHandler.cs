using CleanArchitecture.Cmms.Application.Abstractions.Persistence.Repositories;
using CleanArchitecture.Cmms.Application.Technicians.Dtos;
using CleanArchitecture.Cmms.Domain.Technicians;

namespace CleanArchitecture.Cmms.Application.Technicians.Queries.GetTechnicianById
{
    internal sealed class GetTechnicianByIdQueryHandler
     : IQueryHandler<GetTechnicianByIdQuery, Result<TechnicianDto>>
    {
        private readonly IReadRepository<Technician, Guid> _repository;

        public GetTechnicianByIdQueryHandler(IReadRepository<Technician, Guid> repository)
        {
            _repository = repository;
        }

        public async Task<Result<TechnicianDto>> Handle(GetTechnicianByIdQuery request, CancellationToken cancellationToken)
        {
            var technician = await _repository.GetByIdAsync(request.TechnicianId, cancellationToken);

            if (technician is null)
                return TechnicianErrors.NotFound;

            var dto = new TechnicianDto
            {
                Id = technician.Id,
                Name = technician.Name,
                SkillLevelName = technician.SkillLevel.LevelName,
                SkillLevelRank = technician.SkillLevel.Rank,
                Status = technician.Status.ToString(),
                ActiveAssignmentsCount = technician.Assignments.Count(a => !a.IsCompleted),
                TotalCertifications = technician.Certifications.Count
            };

            return dto;
        }
    }

}
