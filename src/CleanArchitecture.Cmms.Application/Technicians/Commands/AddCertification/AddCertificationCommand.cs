using CleanArchitecture.Core.Application.Abstractions.Common;

namespace CleanArchitecture.Cmms.Application.Technicians.Commands.AddCertification;

public sealed record AddCertificationCommand(Guid TechnicianId, string Code, DateTime IssuedOn, DateTime? ExpiresOn) : ICommand<Result>;
