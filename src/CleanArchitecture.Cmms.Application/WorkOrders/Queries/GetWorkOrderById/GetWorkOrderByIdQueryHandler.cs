using CleanArchitecture.Cmms.Application.WorkOrders.Dtos;
using CleanArchitecture.Cmms.Application.WorkOrders.Interfaces;

namespace CleanArchitecture.Cmms.Application.WorkOrders.Queries.GetWorkOrderById
{
    internal sealed class GetWorkOrderByIdQueryHandler
     : IQueryHandler<GetWorkOrderByIdQuery, Result<WorkOrderDto>>
    {
        private readonly IWorkOrderReadRepository _repository;

        public GetWorkOrderByIdQueryHandler(IWorkOrderReadRepository repository)
        {
            _repository = repository;
        }

        public async Task<Result<WorkOrderDto>> Handle(GetWorkOrderByIdQuery request, CancellationToken cancellationToken)
        {
            var entity = await _repository.GetWorkOrderByIdQuery(request.Id, cancellationToken);

            if (entity is null)
                return $"Work order with ID '{request.Id}' not found.";

            var dto = new WorkOrderDto(entity.Id, entity.Title, entity.Status.ToString());

            return dto;
        }
    }
}
