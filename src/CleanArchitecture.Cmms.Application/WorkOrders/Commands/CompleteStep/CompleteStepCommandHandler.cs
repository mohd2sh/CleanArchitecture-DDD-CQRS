using CleanArchitecture.Cmms.Domain.WorkOrders;
using CleanArchitecture.Core.Application.Abstractions.Common;
using CleanArchitecture.Core.Application.Abstractions.Persistence.Repositories;

namespace CleanArchitecture.Cmms.Application.WorkOrders.Commands.CompleteStep
{
    internal sealed class CompleteStepCommandHandler
     : ICommandHandler<CompleteStepCommand, Result>
    {
        private readonly IRepository<WorkOrder, Guid> _repository;

        public CompleteStepCommandHandler(IRepository<WorkOrder, Guid> repository)
        {
            _repository = repository;
        }

        public async Task<Result> Handle(CompleteStepCommand request, CancellationToken cancellationToken = default)
        {
            var workOrder = await _repository.GetByIdAsync(request.WorkOrderId, cancellationToken);

            if (workOrder is null)
                return WorkOrderErrors.NotFound;

            workOrder.CompleteStep(request.StepId);

            await _repository.UpdateAsync(workOrder, cancellationToken);

            return Result.Success();
        }
    }
}


