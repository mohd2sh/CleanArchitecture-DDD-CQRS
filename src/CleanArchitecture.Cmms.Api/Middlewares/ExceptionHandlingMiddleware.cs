using CleanArchitecture.Cmms.Domain.Abstractions;
using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using System.Net;
using System.Text.Json;

namespace CleanArchitecture.Cmms.Api.Middlewares
{
    public sealed class ExceptionHandlingMiddleware : IMiddleware
    {
        private readonly ILogger<ExceptionHandlingMiddleware> _logger;
        private readonly JsonSerializerOptions _jsonOptions = new() { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
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

            var (status, title, detail) = ex switch
            {
                ValidationException validationEx => (
                    HttpStatusCode.BadRequest,
                    "Validation failed",
                    string.Join("; ", validationEx.Errors.Select(e => $"{e.PropertyName}: {e.ErrorMessage}"))
                ),

                DomainException domainEx => (
                    HttpStatusCode.BadRequest,
                    "Domain rule violated",
                    domainEx.Message
                ),

                KeyNotFoundException keyEx => (
                    HttpStatusCode.NotFound,
                    "Resource not found",
                    keyEx.Message
                ),

                UnauthorizedAccessException => (
                    HttpStatusCode.Unauthorized,
                    "Unauthorized",
                    "Access denied."
                ),

                _ => (
                    HttpStatusCode.InternalServerError,
                    "Internal Server Error",
                    ex.Message
                )
            };

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
