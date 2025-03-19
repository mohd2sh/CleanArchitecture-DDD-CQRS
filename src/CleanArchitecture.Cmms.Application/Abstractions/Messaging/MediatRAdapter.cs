namespace CleanArchitecture.Cmms.Application.Abstractions.Messaging;

internal sealed class MediatRAdapter : IMediator
{
    private readonly MediatR.IMediator _mediator;
    public MediatRAdapter(MediatR.IMediator mediator) => _mediator = mediator;

    public Task Publish<T>(T message, CancellationToken ct = default) where T : INotification
    {
        return _mediator.Publish(message, ct);
    }

    public Task<T> Send<T>(IRequest<T> request, CancellationToken ct = default)
    {
        return _mediator.Send(request, ct);
    }
}
