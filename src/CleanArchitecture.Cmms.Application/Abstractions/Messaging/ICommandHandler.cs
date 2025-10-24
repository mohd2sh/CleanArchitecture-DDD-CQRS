namespace CleanArchitecture.Cmms.Application.Abstractions.Messaging;

public interface ICommandHandler<in TCommand, TResponse>
    where TCommand : ICommand<TResponse>
{
    Task<TResponse> Handle(TCommand request, CancellationToken cancellationToken = default);
}
