using CleanArchitecture.Core.Application.Abstractions.Common;

namespace CleanArchitecture.Cmms.Application.Technicians.Commands.CreateTechnician;

public sealed record CreateTechnicianCommand(string Name, string SkillLevelName, int SkillLevelRank) : ICommand<Result<Guid>>;
