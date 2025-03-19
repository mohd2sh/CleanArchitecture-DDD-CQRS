using Asp.Versioning;
using CleanArchitecture.Cmms.Api.Controllers.V1.Requests;
using CleanArchitecture.Cmms.Application.Abstractions.Messaging;
using CleanArchitecture.Cmms.Application.Primitives;
using CleanArchitecture.Cmms.Application.WorkOrders.Commands.AssignTechnician;
using CleanArchitecture.Cmms.Application.WorkOrders.Commands.CompleteWorkOrder;
using CleanArchitecture.Cmms.Application.WorkOrders.Commands.CreateWorkOrder;
using CleanArchitecture.Cmms.Application.WorkOrders.Dtos;
using CleanArchitecture.Cmms.Application.WorkOrders.Queries.GetActiveWorkOrder;
using Microsoft.AspNetCore.Mvc;
using System.Net;

namespace CleanArchitecture.Cmms.Api.Controllers.V1
{
    [ApiController]
    [ApiVersion("1.0")]
    [Route("api/v{version:apiVersion}/[controller]")]
    public sealed class WorkOrdersController : ControllerBase
    {
        private readonly IMediator _mediator;

        public WorkOrdersController(IMediator mediator)
        {
            _mediator = mediator;
        }


        [HttpPost]
        [ProducesResponseType(typeof(Result<Guid>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(ProblemDetails), (int)HttpStatusCode.BadRequest)]
        [ProducesResponseType(typeof(ProblemDetails), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> Create([FromBody] CreateWorkOrderRequest request, CancellationToken ct)
        {
            var command = new CreateWorkOrderCommand(request.Title, request.Site, request.Area, request.Zone);
            var result = await _mediator.Send(command, ct);
            return result.IsSuccess ? Ok(result) : BadRequest(result.Error);
        }


        [HttpPost("{id:guid}/assign")]
        [ProducesResponseType(typeof(Result), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(ProblemDetails), (int)HttpStatusCode.BadRequest)]
        [ProducesResponseType(typeof(ProblemDetails), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> Assign(Guid id, [FromBody] AssignTechnicianRequest request, CancellationToken ct)
        {
            var command = new AssignTechnicianCommand(id, request.TechnicianId);
            var result = await _mediator.Send(command, ct);
            return result.IsSuccess ? Ok(result) : BadRequest(result.Error);
        }


        [HttpPost("{id:guid}/complete")]
        [ProducesResponseType(typeof(Result), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(ProblemDetails), (int)HttpStatusCode.BadRequest)]
        [ProducesResponseType(typeof(ProblemDetails), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> Complete(Guid id, CancellationToken ct)
        {
            var command = new CompleteWorkOrderCommand(id);
            var result = await _mediator.Send(command, ct);
            return result.IsSuccess ? Ok(result) : BadRequest(result.Error);
        }


        [HttpGet]
        [ProducesResponseType(typeof(PaginatedList<WorkOrderListItemDto>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(ProblemDetails), (int)HttpStatusCode.BadRequest)]
        [ProducesResponseType(typeof(ProblemDetails), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> GetActive([FromQuery] GetActiveWorkOrdersRequest request, CancellationToken ct)
        {
            var query = new GetActiveWorkOrdersQuery(request.Pagination);
            var result = await _mediator.Send(query, ct);
            return Ok(result);
        }


        //[HttpGet("{id:guid}")]
        //[ProducesResponseType(typeof(WorkOrderDetailsDto), (int)HttpStatusCode.OK)]
        //[ProducesResponseType((int)HttpStatusCode.NotFound)]
        //public async Task<IActionResult> GetById(Guid id, CancellationToken ct)
        //{
        //    var query = new GetWorkOrderByIdQuery(id);
        //    var result = await _mediator.Send(query, ct);
        //    return result is not null ? Ok(result) : NotFound();
        //}
    }
}
