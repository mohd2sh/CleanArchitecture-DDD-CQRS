namespace CleanArchitecture.Cmms.Application.Abstractions.Messaging;

public interface ICommandPipeline<in TCommand, TResponse>
    where TCommand : ICommand<TResponse>
{
    Task<TResponse> Handle(
        TCommand request,
        PipelineDelegate<TResponse> next,
        CancellationToken cancellationToken = default);
}
