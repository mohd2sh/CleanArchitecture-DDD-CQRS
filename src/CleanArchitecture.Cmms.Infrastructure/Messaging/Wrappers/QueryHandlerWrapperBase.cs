using CleanArchitecture.Core.Application.Abstractions.Messaging;

namespace CleanArchitecture.Cmms.Infrastructure.Messaging.Wrappers;

/// <summary>
/// Base class for query handler wrappers. Allows CustomMediator to work with 
/// wrappers without knowing the concrete query type at compile time.
/// </summary>
internal abstract class QueryHandlerWrapperBase<TResult>
{
    public abstract Task<TResult> Handle(
        IQuery<TResult> query,
        IServiceProvider serviceProvider,
        CancellationToken cancellationToken);
}

