using CleanArchitecture.Cmms.Application.Technicians.Dtos;
using CleanArchitecture.Core.Application.Abstractions.Common;
using CleanArchitecture.Core.Application.Abstractions.Query;

namespace CleanArchitecture.Cmms.Application.Technicians.Queries.GetTechnicianAssignments;

public sealed record GetTechnicianAssignmentsQuery(Guid TechnicianId, PaginationParam Pagination, bool OnlyActive = false)
  : IQuery<Result<PaginatedList<TechnicianAssignmentDto>>>;
