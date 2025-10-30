using CleanArchitecture.Core.Application.Abstractions.Common;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace CleanArchitecture.Cmms.Api.Configurations;

public sealed class ConfigureApiBehaviorOptions : IConfigureOptions<ApiBehaviorOptions>
{
    public void Configure(ApiBehaviorOptions options)
    {
        options.InvalidModelStateResponseFactory = context =>
        {
            var errors = context.ModelState
                .Where(e => e.Value?.Errors.Count > 0)
                .Select(e => new
                {
                    Field = e.Key,
                    Messages = e.Value!.Errors.Select(x => x.ErrorMessage).ToArray()
                })
                .ToList();

            var errorMessage = string.Join("; ", errors.SelectMany(e => e.Messages));

            var error = Error.Validation("Validation.ModelState", errorMessage);

            var result = Result.Failure(error);

            return new BadRequestObjectResult(result);
        };
    }
}

