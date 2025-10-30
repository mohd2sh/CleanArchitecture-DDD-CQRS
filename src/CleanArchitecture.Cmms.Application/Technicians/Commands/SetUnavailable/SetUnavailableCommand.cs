using CleanArchitecture.Core.Application.Abstractions.Common;

namespace CleanArchitecture.Cmms.Application.Technicians.Commands.SetUnavailable
{
    public sealed record SetUnavailableCommand(Guid TechnicianId) : ICommand<Result>;
}
