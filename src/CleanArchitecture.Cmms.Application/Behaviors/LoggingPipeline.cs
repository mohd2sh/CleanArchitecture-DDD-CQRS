using System.Diagnostics;
using System.Text.Json;
using Microsoft.Extensions.Logging;

namespace CleanArchitecture.Cmms.Application.Behaviors;

public class LoggingPipeline<TRequest, TResult> : IPipeline<TRequest, TResult> where TRequest : notnull
{
    private readonly ILogger<LoggingPipeline<TRequest, TResult>> _logger;

    public LoggingPipeline(ILogger<LoggingPipeline<TRequest, TResult>> logger)
    {
        _logger = logger;
    }

    public async Task<TResult> Handle(TRequest request, PipelineDelegate<TResult> next, CancellationToken cancellationToken = default)
    {
        var requestName = typeof(TRequest).Name;
        var requestJson = JsonSerializer.Serialize(request);

        _logger.LogInformation("Handling {RequestName}: {RequestContent}", requestName, requestJson);

        var stopwatch = Stopwatch.StartNew();
        TResult response;
        try
        {
            response = await next();
        }
        finally
        {
            stopwatch.Stop();
        }

        var responseJson = JsonSerializer.Serialize(response);
        _logger.LogInformation("Handled {RequestName} in {ElapsedMilliseconds}ms. Response: {ResponseContent}",
            requestName, stopwatch.ElapsedMilliseconds, responseJson);

        return response;
    }
}
