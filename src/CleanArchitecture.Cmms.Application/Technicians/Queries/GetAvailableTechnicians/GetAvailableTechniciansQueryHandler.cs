using CleanArchitecture.Cmms.Application.Technicians.Dtos;
using CleanArchitecture.Cmms.Domain.Technicians;
using CleanArchitecture.Cmms.Domain.Technicians.Enums;
using CleanArchitecture.Core.Application.Abstractions.Common;
using CleanArchitecture.Core.Application.Abstractions.Persistence;
using CleanArchitecture.Core.Application.Abstractions.Persistence.Repositories;
using CleanArchitecture.Core.Application.Abstractions.Query;

namespace CleanArchitecture.Cmms.Application.Technicians.Queries.GetAvailableTechnicians
{
    internal sealed class GetAvailableTechniciansQueryHandler
    : IQueryHandler<GetAvailableTechniciansQuery, Result<PaginatedList<TechnicianDto>>>
    {
        private readonly IReadRepository<Technician, Guid> _repository;

        public GetAvailableTechniciansQueryHandler(IReadRepository<Technician, Guid> repository)
        {
            _repository = repository;
        }

        public async Task<Result<PaginatedList<TechnicianDto>>> Handle(GetAvailableTechniciansQuery request, CancellationToken cancellationToken)
        {
            var criteria = Criteria<Technician>.New()
                 .Where(p => p.Status == TechnicianStatus.Available)
                 .OrderByAsc(p => p.Name)
                 .Skip(request.Pagination.PageNumber)
                 .Take(request.Pagination.PageSize)
                 .Build();

            var technicians = await _repository.ListAsync(criteria, cancellationToken);

            var dtoList = technicians.Items.Select(t => new TechnicianDto()
            {
                ActiveAssignmentsCount = t.Assignments.Count,
                Id = t.Id,
                Name = t.Name,
                SkillLevelName = t.SkillLevel.ToString(),
                TotalCertifications = t.Certifications.Count,
            }).ToList();

            return technicians.ToNew(dtoList);
        }
    }

}
