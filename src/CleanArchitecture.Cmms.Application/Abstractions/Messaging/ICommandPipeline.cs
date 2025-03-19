using MediatR;

namespace CleanArchitecture.Cmms.Application.Abstractions.Messaging;

public interface ICommandPipeline<TCommand, TResponse>
    : IPipelineBehavior<TCommand, TResponse>
    where TCommand : ICommand<TResponse>
{ }
