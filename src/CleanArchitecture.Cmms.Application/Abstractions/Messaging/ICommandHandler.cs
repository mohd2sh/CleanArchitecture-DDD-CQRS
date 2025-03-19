namespace CleanArchitecture.Cmms.Application.Abstractions.Messaging;

public interface ICommandHandler<TCommand, TResponse> : MediatR.IRequestHandler<TCommand, TResponse>
    where TCommand : ICommand<TResponse>
{ }
