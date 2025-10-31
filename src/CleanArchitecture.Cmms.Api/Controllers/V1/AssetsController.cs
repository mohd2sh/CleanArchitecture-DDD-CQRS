using System.Net;
using Asp.Versioning;
using CleanArchitecture.Cmms.Api.Controllers.V1.Requests.Assets;
using CleanArchitecture.Cmms.Application.Assets.Commands.CreateAsset;
using CleanArchitecture.Cmms.Application.Assets.Commands.UpdateAssetLocation;
using CleanArchitecture.Cmms.Application.Assets.Dtos;
using CleanArchitecture.Cmms.Application.Assets.Queries.GetActiveAssets;
using CleanArchitecture.Cmms.Application.Assets.Queries.GetAssetById;
using CleanArchitecture.Core.Application.Abstractions.Common;
using CleanArchitecture.Core.Application.Abstractions.Messaging;
using CleanArchitecture.Core.Application.Abstractions.Query;
using Microsoft.AspNetCore.Mvc;

namespace CleanArchitecture.Cmms.Api.Controllers.V1;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/[controller]")]
public sealed class AssetsController : ControllerBase
{
    private readonly IMediator _mediator;

    public AssetsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpPost]
    [ProducesResponseType(typeof(Result<Guid>), (int)HttpStatusCode.OK)]
    public async Task<IActionResult> Create([FromBody] CreateAssetRequest request, CancellationToken cancellationToken)
    {
        var command = new CreateAssetCommand(
            request.Name,
            request.Type,
            request.TagCode,
            request.Site,
            request.Area,
            request.Zone);

        var result = await _mediator.Send(command, cancellationToken);

        return Ok(result);
    }

    [HttpPut("{id:guid}/location")]
    [ProducesResponseType(typeof(Result), (int)HttpStatusCode.OK)]
    public async Task<IActionResult> UpdateLocation(Guid id, [FromBody] UpdateAssetLocationRequest request, CancellationToken cancellationToken)
    {
        var command = new UpdateAssetLocationCommand(id, request.Site, request.Area, request.Zone);

        var result = await _mediator.Send(command, cancellationToken);

        return Ok(result);
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(Result<AssetDto>), (int)HttpStatusCode.OK)]
    public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
    {
        var query = new GetAssetByIdQuery(id);
        var result = await _mediator.Send(query, cancellationToken);
        return Ok(result);
    }

    [HttpGet("active")]
    [ProducesResponseType(typeof(Result<PaginatedList<AssetDto>>), (int)HttpStatusCode.OK)]
    public async Task<IActionResult> GetActive([FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 20, CancellationToken cancellationToken = default)
    {
        var query = new GetActiveAssetsQuery(new PaginationParam(pageNumber, pageSize));
        var result = await _mediator.Send(query, cancellationToken);
        return Ok(result);
    }
}
