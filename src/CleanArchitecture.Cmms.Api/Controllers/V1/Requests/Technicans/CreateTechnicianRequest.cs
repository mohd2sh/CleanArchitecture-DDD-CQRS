namespace CleanArchitecture.Cmms.Api.Controllers.V1.Requests.Technicans;

public sealed class CreateTechnicianRequest
{
    public string Name { get; init; } = default!;
    public string SkillLevelName { get; init; } = default!;
    public int SkillLevelRank { get; init; }
}
