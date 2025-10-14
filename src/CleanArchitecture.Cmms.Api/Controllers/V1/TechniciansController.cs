using Asp.Versioning;
using CleanArchitecture.Cmms.Api.Controllers.V1.Requests.Technicans;
using CleanArchitecture.Cmms.Application.Abstractions.Messaging;
using CleanArchitecture.Cmms.Application.Primitives;
using CleanArchitecture.Cmms.Application.Technicians.Commands.CompleteAssignment;
using CleanArchitecture.Cmms.Application.Technicians.Commands.CreateTechnician;
using CleanArchitecture.Cmms.Application.Technicians.Dtos;
using CleanArchitecture.Cmms.Application.Technicians.Queries.GetAvailableTechnicians;
using CleanArchitecture.Cmms.Application.Technicians.Queries.GetTechnicianAssignments;
using CleanArchitecture.Cmms.Application.Technicians.Queries.GetTechnicianById;
using Microsoft.AspNetCore.Mvc;
using System.Net;

namespace CleanArchitecture.Cmms.Api.Controllers.V1
{
    [ApiController]
    [ApiVersion("1.0")]
    [Route("api/v{version:apiVersion}/[controller]")]
    public sealed class TechniciansController : ControllerBase
    {
        private readonly IMediator _mediator;

        public TechniciansController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpPost]
        [ProducesResponseType(typeof(Result<Guid>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(ProblemDetails), (int)HttpStatusCode.BadRequest)]
        [ProducesResponseType(typeof(ProblemDetails), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> Create([FromBody] CreateTechnicianRequest request, CancellationToken cancellationToken)
        {
            var command = new CreateTechnicianCommand(request.Name, request.SkillLevelName, request.SkillLevelRank);
            var result = await _mediator.Send(command, cancellationToken);
            return result.IsSuccess ? Ok(result) : BadRequest(result.Error);
        }


        [HttpPost("{id:guid}/assignments/{workOrderId:guid}/complete")]
        [ProducesResponseType(typeof(Result), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(ProblemDetails), (int)HttpStatusCode.BadRequest)]
        [ProducesResponseType(typeof(ProblemDetails), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> CompleteAssignment(Guid id, Guid workOrderId, [FromBody] CompleteAssignmentRequest request, CancellationToken cancellationToken)
        {
            var command = new CompleteAssignmentCommand(id, workOrderId, request.CompletedOn);
            var result = await _mediator.Send(command, cancellationToken);
            return result.IsSuccess ? Ok(result) : BadRequest(result.Error);
        }


        [HttpGet("{id:guid}")]
        [ProducesResponseType(typeof(Result<TechnicianDto>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(ProblemDetails), (int)HttpStatusCode.BadRequest)]
        [ProducesResponseType(typeof(ProblemDetails), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
        {
            var query = new GetTechnicianByIdQuery(id);
            var result = await _mediator.Send(query, cancellationToken);
            return result.IsSuccess ? Ok(result) : NotFound(result.Error);
        }

        [HttpGet("available")]
        [ProducesResponseType(typeof(Result<PaginatedList<TechnicianDto>>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(ProblemDetails), (int)HttpStatusCode.BadRequest)]
        [ProducesResponseType(typeof(ProblemDetails), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> GetAvailable([FromQuery] int take = 20, [FromQuery] int skip = 0, CancellationToken cancellationToken = default)
        {
            var query = new GetAvailableTechniciansQuery(take, skip);

            var result = await _mediator.Send(query, cancellationToken);

            return Ok(result);
        }

        [HttpGet("{id:guid}/assignments")]
        [ProducesResponseType(typeof(Result<IReadOnlyList<TechnicianAssignmentDto>>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(ProblemDetails), (int)HttpStatusCode.BadRequest)]
        [ProducesResponseType(typeof(ProblemDetails), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> GetAssignments(Guid id, [FromQuery] int skip, [FromQuery] int take, [FromQuery] bool onlyActive = false, CancellationToken cancellationToken = default)
        {
            var query = new GetTechnicianAssignmentsQuery(id, skip, take, onlyActive);
            var result = await _mediator.Send(query, cancellationToken);
            return result.IsSuccess ? Ok(result) : BadRequest(result.Error);
        }
    }
}
