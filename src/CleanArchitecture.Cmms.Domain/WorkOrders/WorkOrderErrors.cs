using CleanArchitecture.Core.Domain.Abstractions;
using CleanArchitecture.Core.Domain.Abstractions.Attributes;

namespace CleanArchitecture.Cmms.Domain.WorkOrders;

/// <summary>
/// Provides error messages for Work Order domain invariants.
/// </summary>
[ErrorCodeDefinition("WorkOrder")]
public static class WorkOrderErrors
{
    [DomainError]
    public static readonly DomainError TitleRequired = DomainError.Create(
        "WorkOrder.TitleRequired",
        "Work order title cannot be empty.");

    [DomainError]
    public static readonly DomainError AssetIdRequired = DomainError.Create(
        "WorkOrder.AssetIdRequired",
        "Asset ID must be specified.");

    [DomainError]
    public static readonly DomainError InvalidStateTransition = DomainError.Create(
        "WorkOrder.InvalidStateTransition",
        "Invalid state transition.");

    [DomainError]
    public static readonly DomainError StepsNotCompleted = DomainError.Create(
        "WorkOrder.StepsNotCompleted",
        "All steps must be completed.");

    [DomainError]
    public static readonly DomainError TechnicianRequired = DomainError.Create(
        "WorkOrder.TechnicianRequired",
        "Cannot complete work order without technician.");

    [DomainError]
    public static readonly DomainError DescriptionRequired = DomainError.Create(
        "WorkOrder.DescriptionRequired",
        "Description required");

    [DomainError]
    public static readonly DomainError TextRequired = DomainError.Create(
        "WorkOrder.TextRequired",
        "Text required");

    [DomainError]
    public static readonly DomainError StepNotFound = DomainError.Create(
       "WorkOrder.StepNotFound",
       "Step NotFound");
}
