namespace CleanArchitecture.Cmms.Application.WorkOrders.Dtos;

public sealed class WorkOrderListItemDto
{
    public Guid Id { get; init; }
    public string Title { get; init; } = string.Empty;
    public string TechnicianName { get; init; } = string.Empty;
    public string AssetName { get; init; } = string.Empty;
    public string Status { get; init; } = string.Empty;

    public WorkOrderListItemDto() { }
}
