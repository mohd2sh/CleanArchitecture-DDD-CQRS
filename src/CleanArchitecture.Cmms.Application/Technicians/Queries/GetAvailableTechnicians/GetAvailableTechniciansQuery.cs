using CleanArchitecture.Cmms.Application.Abstractions.Common;
using CleanArchitecture.Cmms.Application.Abstractions.Query;
using CleanArchitecture.Cmms.Application.Technicians.Dtos;

namespace CleanArchitecture.Cmms.Application.Technicians.Queries.GetAvailableTechnicians
{
    public sealed record GetAvailableTechniciansQuery(PaginationParam Pagination) : IQuery<Result<PaginatedList<TechnicianDto>>>;

}
