using CleanArchitecture.Core.Application.Abstractions.Common;
using CleanArchitecture.Cmms.Application.Technicians.Dtos;

namespace CleanArchitecture.Cmms.Application.Technicians.Queries.GetTechnicianById
{
    public sealed record GetTechnicianByIdQuery(Guid TechnicianId) : IQuery<Result<TechnicianDto>>;

}
