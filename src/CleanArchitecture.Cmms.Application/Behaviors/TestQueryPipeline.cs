using MediatR;
using Microsoft.Extensions.Logging;

namespace CleanArchitecture.Cmms.Application.Behaviors
{
    internal class TestQueryPipeline<TQuery, TResponse> : IQueryPipeline<TQuery, TResponse> where TQuery : IQuery<TResponse>
    {
        private readonly ILogger<TestQueryPipeline<TQuery, TResponse>> _logger;
        public TestQueryPipeline(ILogger<TestQueryPipeline<TQuery, TResponse>> logger)
        {
            _logger = logger;
        }
        public async Task<TResponse> Handle(TQuery request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Handling query of type {QueryType}", typeof(TQuery).Name);

            return await next();
        }
    }
}
