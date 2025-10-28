using System.Net;
using Asp.Versioning;
using CleanArchitecture.Cmms.Api.Controllers.V1.Requests.Technicans;
using CleanArchitecture.Cmms.Application.Technicians.Commands.AddCertification;
using CleanArchitecture.Cmms.Application.Technicians.Commands.CreateTechnician;
using CleanArchitecture.Cmms.Application.Technicians.Commands.SetAvailable;
using CleanArchitecture.Cmms.Application.Technicians.Commands.SetUnavailable;
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

        [HttpPost("{id:guid}/certifications")]
        [ProducesResponseType(typeof(Result), (int)HttpStatusCode.OK)]
        public async Task<IActionResult> AddCertification(Guid id, [FromBody] AddCertificationRequest request, CancellationToken cancellationToken)
        {
            var command = new AddCertificationCommand(id, request.Code, request.IssuedOn, request.ExpiresOn);

            var result = await _mediator.Send(command, cancellationToken);

            return Ok(result);
        }

        [HttpPost("{id:guid}/set-unavailable")]
        [ProducesResponseType(typeof(Result), (int)HttpStatusCode.OK)]
        public async Task<IActionResult> SetUnavailable(Guid id, CancellationToken cancellationToken)
        {
            var command = new SetUnavailableCommand(id);

            var result = await _mediator.Send(command, cancellationToken);

            return Ok(result);
        }

        [HttpPost("{id:guid}/set-available")]
        [ProducesResponseType(typeof(Result), (int)HttpStatusCode.OK)]
        public async Task<IActionResult> SetAvailable(Guid id, CancellationToken cancellationToken)
        {
            var command = new SetAvailableCommand(id);

            var result = await _mediator.Send(command, cancellationToken);

            return Ok(result);
        }
    }
}
