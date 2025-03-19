using CleanArchitecture.Cmms.Application.Abstractions.Messaging;
using CleanArchitecture.Cmms.Application.Abstractions.Persistence;
using CleanArchitecture.Cmms.Application.Primitives;
using CleanArchitecture.Cmms.Domain.WorkOrders;
using CleanArchitecture.Cmms.Domain.WorkOrders.ValueObjects;

namespace CleanArchitecture.Cmms.Application.WorkOrders.Commands.CreateWorkOrder
{
    internal sealed class CreateWorkOrderCommandHandler : ICommandHandler<CreateWorkOrderCommand, Result<Guid>>
    {
        private readonly IRepository<WorkOrder, Guid> _repository;

        public CreateWorkOrderCommandHandler(IRepository<WorkOrder, Guid> repository)
        {
            _repository = repository;
        }

        public async Task<Result<Guid>> Handle(CreateWorkOrderCommand request, CancellationToken ct)
        {
            Location location = Location.Create(request.Building, request.Floor, request.Room);

            WorkOrder workOrder = WorkOrder.Create(request.Title, location);

            await _repository.AddAsync(workOrder, ct);

            return workOrder.Id;
        }
    }
}
