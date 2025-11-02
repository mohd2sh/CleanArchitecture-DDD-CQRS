using CleanArchitecture.Core.Application.Abstractions.Common;

namespace CleanArchitecture.Cmms.Application.WorkOrders.Commands.AddStep;

public sealed record AddStepCommand(Guid WorkOrderId, string Title) : ICommand<Result<Guid>>;
