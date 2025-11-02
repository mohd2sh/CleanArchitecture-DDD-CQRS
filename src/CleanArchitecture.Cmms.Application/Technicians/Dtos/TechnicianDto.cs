namespace CleanArchitecture.Cmms.Application.Technicians.Dtos;

public sealed class TechnicianDto
{
    public Guid Id { get; init; }
    public string Name { get; init; } = default!;
    public string SkillLevelName { get; init; } = default!;
    public int ActiveAssignmentsCount { get; init; }
    public int TotalCertifications { get; init; }
    public int SkillLevelRank { get; init; }
    public string Status { get; init; } = default!;

    public TechnicianDto() { }
}
