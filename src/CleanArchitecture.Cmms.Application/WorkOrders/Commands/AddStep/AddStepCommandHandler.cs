using CleanArchitecture.Cmms.Domain.WorkOrders;
using CleanArchitecture.Core.Application.Abstractions.Common;
using CleanArchitecture.Core.Application.Abstractions.Persistence.Repositories;

namespace CleanArchitecture.Cmms.Application.WorkOrders.Commands.AddStep
{
    internal sealed class AddStepCommandHandler
     : ICommandHandler<AddStepCommand, Result<Guid>>
    {
        private readonly IRepository<WorkOrder, Guid> _repository;

        public AddStepCommandHandler(IRepository<WorkOrder, Guid> repository)
        {
            _repository = repository;
        }

        public async Task<Result<Guid>> Handle(AddStepCommand request, CancellationToken cancellationToken = default)
        {
            var workOrder = await _repository.GetByIdAsync(request.WorkOrderId, cancellationToken);

            if (workOrder is null)
                return WorkOrderErrors.NotFound;

            var stepId = workOrder.AddStep(request.Title);

            await _repository.UpdateAsync(workOrder, cancellationToken);

            return stepId;
        }
    }

}
