using CleanArchitecture.Core.Application.Abstractions.Events;
using CleanArchitecture.Core.Domain.Abstractions;

namespace CleanArchitecture.Core.Infrastructure.Messaging.Conventions;

/// <summary>
/// Default convention for identifying integration events.
/// </summary>
internal sealed class DefaultIntegrationEventConvention : IIntegrationEventConvention
{
    public bool IsIntegrationEvent(Type type)
    {
        ArgumentNullException.ThrowIfNull(type);

        if (type.IsAbstract || type.IsPrimitive)
            return false;

        if (type.Name.EndsWith("Event", StringComparison.Ordinal))
            return true;

        if (typeof(IDomainEvent).IsAssignableFrom(type))
            return true;

        if (type.Namespace != null &&
            (type.Namespace.EndsWith(".Events", StringComparison.Ordinal) ||
             type.Namespace.EndsWith(".IntegrationEvents", StringComparison.Ordinal)))
            return true;

        return false;
    }
}
