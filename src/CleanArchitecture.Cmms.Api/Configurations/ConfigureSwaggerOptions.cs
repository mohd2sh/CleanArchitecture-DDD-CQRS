using Asp.Versioning.ApiExplorer;
using Microsoft.Extensions.Options;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace CleanArchitecture.Cmms.Api.Configurations;

public sealed class ConfigureSwaggerOptions : IConfigureOptions<SwaggerGenOptions>
{
    private readonly IApiVersionDescriptionProvider _provider;

    public ConfigureSwaggerOptions(IApiVersionDescriptionProvider provider)
    {
        _provider = provider;
    }

    public void Configure(SwaggerGenOptions options)
    {
        foreach (var description in _provider.ApiVersionDescriptions)
        {
            options.SwaggerDoc(
                description.GroupName,
                new OpenApiInfo
                {
                    Title = "CMMS API",
                    Version = description.ApiVersion.ToString(),
                    Description = "Clean Architecture + DDD example with CQRS and EF Core",
                    Contact = new OpenApiContact
                    {
                        Name = "Mohd2sh/CleanArchitecture-DDD-CQRS",
                        Url = new Uri("https://github.com/mohd2sh/CleanArchitecture-DDD-CQRS")
                    }
                });
        }

        // Add operation filter for Result responses
        options.OperationFilter<ResultResponseOperationFilter>();

        // Optional JWT setup for future
        options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
        {
            Description = "JWT Authorization header using the Bearer scheme.",
            Name = "Authorization",
            In = ParameterLocation.Header,
            Type = SecuritySchemeType.ApiKey
        });

        options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Bearer" }
            },
            Array.Empty<string>()
        }
    });
    }
}
