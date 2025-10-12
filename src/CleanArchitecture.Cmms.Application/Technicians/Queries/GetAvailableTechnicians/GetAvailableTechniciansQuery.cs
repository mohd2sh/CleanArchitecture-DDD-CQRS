using CleanArchitecture.Cmms.Application.Technicians.Dtos;

namespace CleanArchitecture.Cmms.Application.Technicians.Queries.GetAvailableTechnicians
{
    public sealed record GetAvailableTechniciansQuery(int Take, int Skip) : IQuery<PaginatedList<TechnicianDto>>;

}
