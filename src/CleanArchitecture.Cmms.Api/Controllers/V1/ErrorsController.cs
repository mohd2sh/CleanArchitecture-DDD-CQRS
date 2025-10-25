using Asp.Versioning;
using CleanArchitecture.Cmms.Application.Primitives;
using Microsoft.AspNetCore.Mvc;

namespace CleanArchitecture.Cmms.Api.Controllers.V1;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/[controller]")]
public class ErrorsController : ControllerBase
{
    /// <summary>
    /// Exports all error codes and messages for client-side localization.
    /// Includes both domain and application errors discovered via attributes.
    /// </summary>
    [HttpGet("export")]
    [ProducesResponseType(typeof(ErrorExportResult), StatusCodes.Status200OK)]
    public IActionResult ExportAll()
    {
        var result = ErrorExporter.ExportAll();
        return Ok(result);
    }

    /// <summary>
    /// Exports only application-level errors.
    /// </summary>
    [HttpGet("application")]
    [ProducesResponseType(typeof(Dictionary<string, ApplicationErrorInfo>), StatusCodes.Status200OK)]
    public IActionResult ExportApplicationErrors()
    {
        var errors = ErrorExporter.ExportApplicationErrors();
        return Ok(errors);
    }

    /// <summary>
    /// Exports only domain-level errors.
    /// </summary>
    [HttpGet("domain")]
    [ProducesResponseType(typeof(Dictionary<string, DomainErrorInfo>), StatusCodes.Status200OK)]
    public IActionResult ExportDomainErrors()
    {
        var errors = ErrorExporter.ExportDomainErrors();
        return Ok(errors);
    }
}
