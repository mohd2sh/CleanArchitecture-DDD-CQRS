using CleanArchitecture.Cmms.Application.Abstractions.Persistence.Repositories;
using CleanArchitecture.Cmms.Domain.WorkOrders;

namespace CleanArchitecture.Cmms.Application.WorkOrders.Commands.AddStep
{
    internal sealed class AddStepCommandHandler
     : ICommandHandler<AddStepCommand, Result>
    {
        private readonly IRepository<WorkOrder, Guid> _repository;

        public AddStepCommandHandler(IRepository<WorkOrder, Guid> repository)
        {
            _repository = repository;
        }

        public async Task<Result> Handle(AddStepCommand request, CancellationToken cancellationToken)
        {
            var workOrder = await _repository.GetByIdAsync(request.WorkOrderId, cancellationToken);

            if (workOrder is null)
                return "Work order not found.";

            workOrder.AddStep(request.Title);

            await _repository.UpdateAsync(workOrder, cancellationToken);

            return Result.Success();
        }
    }

}
