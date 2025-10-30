using CleanArchitecture.Core.Application.Abstractions.Common;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace CleanArchitecture.Cmms.Api.Filters;

public class ResultToHttpStatusFilter : IAsyncResultFilter
{
    public async Task OnResultExecutionAsync(ResultExecutingContext context, ResultExecutionDelegate next)
    {
        if (context.Result is ObjectResult objectResult &&
            objectResult.Value is Result result)
        {
            if (!result.IsSuccess && result.Error != null)
            {
                objectResult.StatusCode = result.Error.Type switch
                {
                    ErrorType.Validation => StatusCodes.Status400BadRequest,
                    ErrorType.NotFound => StatusCodes.Status404NotFound,
                    ErrorType.Conflict => StatusCodes.Status409Conflict,
                    ErrorType.Unauthorized => StatusCodes.Status401Unauthorized,
                    ErrorType.Forbidden => StatusCodes.Status403Forbidden,
                    ErrorType.Failure => StatusCodes.Status400BadRequest,
                    _ => StatusCodes.Status400BadRequest
                };
            }
        }

        await next();
    }
}
