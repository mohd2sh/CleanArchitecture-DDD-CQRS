namespace CleanArchitecture.Cmms.Application.WorkOrders.Dtos;

public sealed record WorkOrderDto(
    Guid Id,
    string Title,
    string Status
);
