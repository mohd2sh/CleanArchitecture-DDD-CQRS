using CleanArchitecture.Cmms.Application.Technicians.Dtos;

namespace CleanArchitecture.Cmms.Application.Technicians.Queries.GetTechnicianAssignments
{
    public sealed record GetTechnicianAssignmentsQuery(Guid TechnicianId, PaginationParam Pagination, bool OnlyActive = false)
      : IQuery<Result<PaginatedList<TechnicianAssignmentDto>>>;

}
