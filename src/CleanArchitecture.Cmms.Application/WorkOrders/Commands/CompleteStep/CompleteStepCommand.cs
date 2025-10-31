using CleanArchitecture.Core.Application.Abstractions.Common;

namespace CleanArchitecture.Cmms.Application.WorkOrders.Commands.CompleteStep;

public sealed record CompleteStepCommand(Guid WorkOrderId, Guid StepId) : ICommand<Result>;

