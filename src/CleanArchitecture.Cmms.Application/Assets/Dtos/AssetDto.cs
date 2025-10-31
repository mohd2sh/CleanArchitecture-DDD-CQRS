namespace CleanArchitecture.Cmms.Application.Assets.Dtos;

public sealed class AssetDto
{
    public Guid Id { get; init; }
    public string Name { get; init; } = default!;
    public string Type { get; init; } = default!;
    public string TagValue { get; init; } = default!;
    public string Site { get; init; } = default!;
    public string Area { get; init; } = default!;
    public string Zone { get; init; } = default!;
    public string Status { get; init; } = default!;
    public int TotalMaintenanceRecords { get; init; }
}
