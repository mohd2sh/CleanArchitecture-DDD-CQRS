using CleanArchitecture.Cmms.Application.Abstractions.Persistence;
using CleanArchitecture.Cmms.Application.Abstractions.Persistence.Repositories;
using CleanArchitecture.Cmms.Application.Technicians.Dtos;
using CleanArchitecture.Cmms.Domain.Technicians;

namespace CleanArchitecture.Cmms.Application.Technicians.Queries.GetTechnicianAssignments
{
    internal sealed class GetTechnicianAssignmentsQueryHandler
     : IQueryHandler<GetTechnicianAssignmentsQuery, Result<PaginatedList<TechnicianAssignmentDto>>>
    {
        private readonly IReadRepository<Technician, Guid> _repository;

        public GetTechnicianAssignmentsQueryHandler(IReadRepository<Technician, Guid> repository)
        {
            _repository = repository;
        }

        public async Task<Result<PaginatedList<TechnicianAssignmentDto>>> Handle(
            GetTechnicianAssignmentsQuery request,
            CancellationToken cancellationToken)
        {
            var criteria = Criteria<Technician>.New()
                .Where(p => p.Id == request.TechnicianId)
                .Include(p => p.Assignments)
                .OrderByAsc(p => p.Name)
                .Skip(request.Pagination.Skip)
                .Take(request.Pagination.Take)
                .Build();

            var technicians = await _repository.ListAsync(criteria, cancellationToken);
            if (technicians == null || technicians.Items.Count == 0)
                return TechnicianErrors.NotFound;

            var assignmentsDto = technicians.Items
                .SelectMany(t => t.Assignments)
                .Select(a => new TechnicianAssignmentDto
                {
                    WorkOrderId = a.Id,
                    AssignedOn = a.AssignedOn,
                    CompletedOn = a.CompletedOn
                })
                .ToList();

            return technicians.ToNew(assignmentsDto);
        }
    }
}
