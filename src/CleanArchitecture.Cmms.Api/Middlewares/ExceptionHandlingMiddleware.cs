using System.Net;
using System.Text.Json;
using CleanArchitecture.Cmms.Application.Primitives;
using CleanArchitecture.Cmms.Domain.Abstractions;
using FluentValidation;
using Microsoft.AspNetCore.Mvc;

namespace CleanArchitecture.Cmms.Api.Middlewares
{
    public sealed class ExceptionHandlingMiddleware : IMiddleware
    {
        private readonly ILogger<ExceptionHandlingMiddleware> _logger;
        private readonly JsonSerializerOptions _jsonOptions = new()
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = false
        };

        public ExceptionHandlingMiddleware(ILogger<ExceptionHandlingMiddleware> logger)
            => _logger = logger;

        public async Task InvokeAsync(HttpContext context, RequestDelegate next)
        {
            try
            {
                await next(context);
            }
            catch (Exception ex)
            {
                await HandleExceptionAsync(context, ex);
            }
        }

        private async Task HandleExceptionAsync(HttpContext context, Exception ex)
        {
            _logger.LogError(ex, "Unhandled exception occurred.");

            switch (ex)
            {
                case DomainException domainEx:
                    await WriteResultResponseAsync(context, domainEx);
                    break;

                case ValidationException validationEx:
                    await WriteResultResponseAsync(context, validationEx);
                    break;

                case KeyNotFoundException keyEx:
                    await WriteProblemResponseAsync(
                        context,
                        HttpStatusCode.NotFound,
                        "Resource not found",
                        keyEx.Message
                    );
                    break;

                case UnauthorizedAccessException:
                    await WriteProblemResponseAsync(
                        context,
                        HttpStatusCode.Unauthorized,
                        "Unauthorized",
                        "Access denied."
                    );
                    break;

                default:
                    await WriteProblemResponseAsync(
                        context,
                        HttpStatusCode.InternalServerError,
                        "Internal Server Error",
                        "An unexpected error occurred."
                    );
                    break;
            }
        }

        private async Task WriteResultResponseAsync(HttpContext context, Exception ex)
        {
            var result = Result.Failure(ex.Message);

            context.Response.ContentType = "application/json";
            context.Response.StatusCode = (int)HttpStatusCode.BadRequest;

            await context.Response.WriteAsync(JsonSerializer.Serialize(result, _jsonOptions));
        }

        private async Task WriteProblemResponseAsync(HttpContext context, HttpStatusCode status, string title, string detail)
        {
            var problem = new ProblemDetails
            {
                Title = title,
                Detail = detail,
                Status = (int)status,
                Instance = context.Request.Path
            };

            context.Response.ContentType = "application/problem+json";
            context.Response.StatusCode = (int)status;

            await context.Response.WriteAsync(JsonSerializer.Serialize(problem, _jsonOptions));
        }
    }
}
