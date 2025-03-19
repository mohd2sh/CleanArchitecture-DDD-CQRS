namespace CleanArchitecture.Cmms.Application.Abstractions.Messaging;

public interface IMediator
{
    Task<T> Send<T>(IRequest<T> request, CancellationToken ct = default);
    Task Publish<T>(T message, CancellationToken ct = default) where T : INotification;
}
