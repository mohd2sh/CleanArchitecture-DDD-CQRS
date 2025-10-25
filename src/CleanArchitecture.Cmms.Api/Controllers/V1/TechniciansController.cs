using System.Net;
using Asp.Versioning;
using CleanArchitecture.Cmms.Api.Controllers.V1.Requests.Technicans;
using CleanArchitecture.Cmms.Application.Technicians.Commands.CreateTechnician;
using CleanArchitecture.Cmms.Application.Technicians.Dtos;
using CleanArchitecture.Cmms.Application.Technicians.Queries.GetAvailableTechnicians;
using CleanArchitecture.Cmms.Application.Technicians.Queries.GetTechnicianAssignments;
using CleanArchitecture.Cmms.Application.Technicians.Queries.GetTechnicianById;
using CleanArchitecture.Core.Application.Abstractions.Common;
using CleanArchitecture.Core.Application.Abstractions.Messaging;
using CleanArchitecture.Core.Application.Abstractions.Query;
using Microsoft.AspNetCore.Mvc;

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
        public async Task<IActionResult> Create([FromBody] CreateTechnicianRequest request, CancellationToken cancellationToken)
        {
            var command = new CreateTechnicianCommand(request.Name, request.SkillLevelName, request.SkillLevelRank);

            var result = await _mediator.Send(command, cancellationToken);

            return Ok(result);
        }

        [HttpGet("{id:guid}")]
        [ProducesResponseType(typeof(Result<TechnicianDto>), (int)HttpStatusCode.OK)]
        public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
        {
            var query = new GetTechnicianByIdQuery(id);

            var result = await _mediator.Send(query, cancellationToken);

            return Ok(result);
        }

        [HttpGet("available")]
        [ProducesResponseType(typeof(Result<PaginatedList<TechnicianDto>>), (int)HttpStatusCode.OK)]
        public async Task<IActionResult> GetAvailable([FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 20, CancellationToken cancellationToken = default)
        {
            var query = new GetAvailableTechniciansQuery(new PaginationParam(pageNumber, pageSize));

            var result = await _mediator.Send(query, cancellationToken);

            return Ok(result);
        }

        [HttpGet("{id:guid}/assignments")]
        [ProducesResponseType(typeof(Result<PaginatedList<TechnicianAssignmentDto>>), (int)HttpStatusCode.OK)]
        public async Task<IActionResult> GetAssignments(Guid id, [FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 20, [FromQuery] bool onlyActive = false, CancellationToken cancellationToken = default)
        {
            var query = new GetTechnicianAssignmentsQuery(id, new PaginationParam(pageNumber, pageSize), onlyActive);

            var result = await _mediator.Send(query, cancellationToken);

            return Ok(result);
        }
    }
}
