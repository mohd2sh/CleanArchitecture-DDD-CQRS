namespace CleanArchitecture.Cmms.Application.Abstractions.Messaging;

public interface INotificationHandler<in TNotification> : MediatR.INotificationHandler<TNotification>
    where TNotification : INotification
{ }
