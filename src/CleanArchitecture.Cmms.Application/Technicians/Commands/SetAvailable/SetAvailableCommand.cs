using CleanArchitecture.Core.Application.Abstractions.Common;

namespace CleanArchitecture.Cmms.Application.Technicians.Commands.SetAvailable;

public sealed record SetAvailableCommand(Guid TechnicianId) : ICommand<Result>;
