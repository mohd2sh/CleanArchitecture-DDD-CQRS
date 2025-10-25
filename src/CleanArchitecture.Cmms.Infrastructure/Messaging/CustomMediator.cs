using CleanArchitecture.Cmms.Infrastructure.Messaging.Wrappers;
using CleanArchitecture.Core.Application.Abstractions.Events;
using CleanArchitecture.Core.Application.Abstractions.Messaging;
using CleanArchitecture.Core.Domain.Abstractions;
using Microsoft.Extensions.DependencyInjection;

namespace CleanArchitecture.Cmms.Infrastructure.Messaging;

internal sealed class CustomMediator : IMediator
{
    private readonly IServiceProvider _serviceProvider;

    public CustomMediator(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public Task<TResult> Send<TResult>(ICommand<TResult> command, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(command);

        // Get the runtime command type
        var commandType = command.GetType();

        // Construct the wrapper type: CommandHandlerWrapper<TCommand, TResult>
        var wrapperType = typeof(CommandHandlerWrapper<,>).MakeGenericType(commandType, typeof(TResult));

        // Create wrapper instance (wrapper is not registered, so we create it)
        var wrapper = (CommandHandlerWrapperBase<TResult>)Activator.CreateInstance(wrapperType)!;

        // Delegate to the wrapper - it has full type information
        return wrapper.Handle(command, _serviceProvider, cancellationToken);
    }

    public Task<TResult> Send<TResult>(IQuery<TResult> query, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(query);

        // Get the runtime query type
        var queryType = query.GetType();

        // Construct the wrapper type: QueryHandlerWrapper<TQuery, TResult>
        var wrapperType = typeof(QueryHandlerWrapper<,>).MakeGenericType(queryType, typeof(TResult));

        // Create wrapper instance
        var wrapper = (QueryHandlerWrapperBase<TResult>)Activator.CreateInstance(wrapperType)!;

        // Delegate to the wrapper
        return wrapper.Handle(query, _serviceProvider, cancellationToken);
    }

    public async Task Publish(IDomainEvent domainEvent, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(domainEvent);

        var dispatcher = _serviceProvider.GetRequiredService<IDomainEventDispatcher>();

        await dispatcher.PublishAsync(domainEvent, cancellationToken);
    }
}
