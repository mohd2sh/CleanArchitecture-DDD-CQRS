namespace CleanArchitecture.Cmms.Application.Abstractions.Messaging;

public interface ICommandPipeline<TCommand, TResponse> : IPipeline<TCommand, TResponse>
    where TCommand : ICommand<TResponse>
{

}
