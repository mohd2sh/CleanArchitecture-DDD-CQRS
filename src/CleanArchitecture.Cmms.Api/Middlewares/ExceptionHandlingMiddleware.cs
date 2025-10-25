using System.Net;
using System.Text.Json;
using CleanArchitecture.Cmms.Application.Abstractions.Common;
using CleanArchitecture.Cmms.Application.Assets;
using CleanArchitecture.Cmms.Domain.Abstractions;
using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

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

                case DbUpdateConcurrencyException concurrencyEx:
                    await WriteConcurrencyResponseAsync(context, concurrencyEx);
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
            var error = ex switch
            {
                DomainException domainEx =>
                    Error.Failure(domainEx.Error.Code, domainEx.Error.Message),
                ValidationException => Error.Validation("Validation.Failure", ex.Message),
                _ => Error.Failure("General.Failure", ex.Message)
            };

            var result = Result.Failure(error);

            context.Response.ContentType = "application/json";
            context.Response.StatusCode = (int)HttpStatusCode.BadRequest;

            await context.Response.WriteAsync(JsonSerializer.Serialize(result, _jsonOptions));
        }

        private async Task WriteConcurrencyResponseAsync(HttpContext context, DbUpdateConcurrencyException ex)
        {
            _logger.LogWarning(ex, "Concurrency conflict occurred.");

            var result = Result.Failure(AssetErrors.ConcurrencyConflict);

            context.Response.ContentType = "application/json";
            context.Response.StatusCode = (int)HttpStatusCode.Conflict;

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
