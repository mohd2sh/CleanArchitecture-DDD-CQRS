namespace CleanArchitecture.Cmms.Application.Assets.Dtos
{
    public sealed class MaintenanceRecordDto
    {
        public Guid Id { get; init; }
        public DateTime StartedOn { get; init; }
        public string Description { get; init; } = default!;
        public string PerformedBy { get; init; } = default!;
    }
}
