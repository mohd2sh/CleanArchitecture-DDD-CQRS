using CleanArchitecture.Cmms.Domain.Abstractions;

namespace CleanArchitecture.Cmms.Application.Abstractions.Messaging.Models
{
    public sealed class DomainEventNotification<T> : INotification
     where T : IDomainEvent
    {
        public T DomainEvent { get; }

        // Parameterless ctor → reflection/serialization friendly
        public DomainEventNotification() { }

        public DomainEventNotification(T domainEvent)
            => DomainEvent = domainEvent;

        public static INotification Create(IDomainEvent domainEvent)
        {
            var type = domainEvent.GetType();

            var genericType = typeof(DomainEventNotification<>).MakeGenericType(type);

            return (INotification)Activator.CreateInstance(genericType, domainEvent)!;
        }
    }

}
