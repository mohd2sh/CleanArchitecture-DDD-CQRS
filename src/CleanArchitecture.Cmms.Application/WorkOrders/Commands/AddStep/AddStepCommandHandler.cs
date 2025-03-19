using CleanArchitecture.Cmms.Application.Abstractions.Messaging;
using CleanArchitecture.Cmms.Application.Abstractions.Persistence;
using CleanArchitecture.Cmms.Application.Primitives;
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

        public async Task<Result> Handle(AddStepCommand request, CancellationToken ct)
        {
            var workOrder = await _repository.GetByIdAsync(request.WorkOrderId, ct);

            if (workOrder is null)
                return "Work order not found.";

            workOrder.AddStep(request.Title);

            await _repository.UpdateAsync(workOrder, ct);

            return Result.Success();
        }
    }

}
