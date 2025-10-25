using CleanArchitecture.Cmms.Application.Abstractions.Common;
using Microsoft.AspNetCore.Mvc;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace CleanArchitecture.Cmms.Api.Configurations;

public class ResultResponseOperationFilter : IOperationFilter
{
    public void Apply(OpenApiOperation operation, OperationFilterContext context)
    {
        // Add standard error responses to all endpoints
        AddResponseIfNotExists(operation, "400", "Bad Request - Validation or business rule failure", typeof(Result));
        AddResponseIfNotExists(operation, "404", "Not Found - Resource not found", typeof(Result));
        AddResponseIfNotExists(operation, "409", "Conflict - Resource conflict", typeof(Result));
        AddResponseIfNotExists(operation, "500", "Internal Server Error", typeof(ProblemDetails));
        AddResponseIfNotExists(operation, "401", "Unauthorized", typeof(ProblemDetails));
    }

    private static void AddResponseIfNotExists(OpenApiOperation operation, string statusCode,
        string description, Type responseType)
    {
        if (!operation.Responses.ContainsKey(statusCode))
        {
            OpenApiSchema schema;

            // Handle ProblemDetails with inline schema
            if (responseType == typeof(ProblemDetails))
            {
                schema = new OpenApiSchema
                {
                    Type = "object",
                    Properties = new Dictionary<string, OpenApiSchema>
                    {
                        ["type"] = new OpenApiSchema { Type = "string" },
                        ["title"] = new OpenApiSchema { Type = "string" },
                        ["status"] = new OpenApiSchema { Type = "integer", Format = "int32" },
                        ["detail"] = new OpenApiSchema { Type = "string" },
                        ["instance"] = new OpenApiSchema { Type = "string" }
                    }
                };
            }
            else
            {
                // Use reference for other types like Result
                schema = new OpenApiSchema
                {
                    Reference = new OpenApiReference
                    {
                        Type = ReferenceType.Schema,
                        Id = responseType.Name
                    }
                };
            }

            operation.Responses.Add(statusCode, new OpenApiResponse
            {
                Description = description,
                Content = new Dictionary<string, OpenApiMediaType>
                {
                    ["application/json"] = new OpenApiMediaType
                    {
                        Schema = schema
                    }
                }
            });
        }
    }
}
