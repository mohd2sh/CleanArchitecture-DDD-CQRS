namespace CleanArchitecture.Cmms.Application.WorkOrders;

using CleanArchitecture.Core.Application.Abstractions.Common;
using CleanArchitecture.Core.Domain.Abstractions.Attributes;

/// <summary>
/// Provides centralized error definitions for Work Order operations.
/// </summary>
[ErrorCodeDefinition("WorkOrder")]
public static class WorkOrderErrors
{
    // NotFound Errors
    [ApplicationError]
    public static readonly Error NotFound = Error.NotFound(
        "WorkOrder.NotFound",
        "Work order not found.");

    // Validation Errors (reusing domain messages for consistency)
    [ApplicationError]
    public static readonly Error TitleRequired = Error.Validation(
        "WorkOrder.TitleRequired",
        Domain.WorkOrders.WorkOrderErrors.TitleRequired.Message);

    [ApplicationError]
    public static readonly Error AssetRequired = Error.Validation(
        "WorkOrder.AssetIdRequired",
        Domain.WorkOrders.WorkOrderErrors.AssetIdRequired.Message);

    [ApplicationError]
    public static readonly Error TechnicianRequired = Error.Validation(
        "WorkOrder.TechnicianRequired",
        Domain.WorkOrders.WorkOrderErrors.TechnicianRequired.Message);

    [ApplicationError]
    public static readonly Error InvalidStateTransition = Error.Validation(
        "WorkOrder.InvalidStateTransition",
        Domain.WorkOrders.WorkOrderErrors.InvalidStateTransition.Message);

    [ApplicationError]
    public static readonly Error StepsNotCompleted = Error.Validation(
        "WorkOrder.StepsNotCompleted",
        Domain.WorkOrders.WorkOrderErrors.StepsNotCompleted.Message);
}
