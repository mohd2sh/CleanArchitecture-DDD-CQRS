using CleanArchitecture.Core.Application.Abstractions.Events;
using CleanArchitecture.Core.Domain.Abstractions;
using CleanArchitecture.Core.Infrastructure.Messaging.Wrappers;
using Microsoft.Extensions.Logging;

namespace CleanArchitecture.Core.Infrastructure.Messaging;

/// <summary>
/// Dispatcher for integration events using wrapper pattern for strongly-typed handler invocation.
/// Uses conventions to identify integration events (unobtrusive mode).
/// </summary>
internal sealed class IntegrationEventDispatcher : IIntegrationEventDispatcher
{
    private readonly IServiceProvider _serviceProvider;
    private readonly IIntegrationEventConvention _eventConvention;
    private readonly ILogger<IntegrationEventDispatcher>? _logger;

    public IntegrationEventDispatcher(
        IServiceProvider serviceProvider,
        IIntegrationEventConvention eventConvention)
    {
        _serviceProvider = serviceProvider;
        _eventConvention = eventConvention;
        _logger = serviceProvider.GetService(typeof(ILogger<IntegrationEventDispatcher>)) as ILogger<IntegrationEventDispatcher>;
    }

    public async Task PublishAsync(IDomainEvent domainEvent, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(domainEvent);

        await PublishAsync((object)domainEvent, cancellationToken);
    }

    public async Task PublishAsync(object integrationEvent, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(integrationEvent);

        var eventType = integrationEvent.GetType();

        // Check if event type matches integration event conventions
        if (!_eventConvention.IsIntegrationEvent(eventType))
        {
            _logger?.LogWarning(
                "Event type {EventType} does not match integration event conventions. Skipping handler invocation.",
                eventType.FullName);
            return;
        }

        var wrapperType = typeof(IntegrationEventHandlerWrapper<>).MakeGenericType(eventType);

        var wrapper = (IntegrationEventHandlerWrapperBase)Activator.CreateInstance(wrapperType)!;

        await wrapper.Handle(integrationEvent, _serviceProvider, cancellationToken);
    }
}

