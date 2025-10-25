using CleanArchitecture.Cmms.Application.Abstractions.Messaging;

namespace CleanArchitecture.Cmms.Infrastructure.Messaging.Wrappers;

/// <summary>
/// Base class for command handler wrappers. Allows CustomMediator to work with 
/// wrappers without knowing the concrete command type at compile time.
/// </summary>
internal abstract class CommandHandlerWrapperBase<TResult>
{
    public abstract Task<TResult> Handle(
        ICommand<TResult> command,
        IServiceProvider serviceProvider,
        CancellationToken cancellationToken);
}

