using CleanArchitecture.Cmms.Application.Technicians.Dtos;

namespace CleanArchitecture.Cmms.Application.Technicians.Queries.GetTechnicianAssignments
{
    public sealed record GetTechnicianAssignmentsQuery(Guid TechnicianId, int Skip, int Take, bool OnlyActive = false)
      : IQuery<Result<PaginatedList<TechnicianAssignmentDto>>>;

}
