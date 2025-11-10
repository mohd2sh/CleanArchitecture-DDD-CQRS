using System.Reflection;
using CleanArchitecture.Core.Application.Abstractions.Events;
using CleanArchitecture.Core.Application.Abstractions.Messaging;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace CleanArchitecture.Core.Application;

/// <summary>
/// Extension methods for configuring Application handlers.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Adds application handlers (commands, queries, domain events, integration events) from the assembly
    /// containing the specified type marker.
    /// </summary>
    /// <param name="services">The <see cref="IServiceCollection"/> to add services to.</param>
    /// <param name="handlerAssemblyMarkerType">A type from the assembly containing handlers.</param>
    /// <returns>The <see cref="IServiceCollection"/> so that additional calls can be chained.</returns>
    public static IServiceCollection AddApplicationHandlers(
        this IServiceCollection services,
        Type handlerAssemblyMarkerType)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(handlerAssemblyMarkerType);

        var assembly = handlerAssemblyMarkerType.Assembly;
        return services.AddApplicationHandlers(assembly);
    }

    /// <summary>
    /// Adds application handlers (commands, queries, domain events, integration events) from the assembly
    /// containing the specified type marker with optional configuration.
    /// </summary>
    /// <param name="services">The <see cref="IServiceCollection"/> to add services to.</param>
    /// <param name="handlerAssemblyMarkerType">A type from the assembly containing handlers.</param>
    /// <param name="configure">An optional action to configure the <see cref="ApplicationHandlerOptions"/>.</param>
    /// <returns>The <see cref="IServiceCollection"/> so that additional calls can be chained.</returns>
    public static IServiceCollection AddApplicationHandlers(
        this IServiceCollection services,
        Type handlerAssemblyMarkerType,
        Action<ApplicationHandlerOptions>? configure)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(handlerAssemblyMarkerType);

        var assembly = handlerAssemblyMarkerType.Assembly;
        var options = new ApplicationHandlerOptions();
        configure?.Invoke(options);

        RegisterCommandAndQueryHandlers(services, assembly);
        RegisterDomainEventHandlers(services, assembly);
        RegisterIntegrationEventHandlers(services, assembly);

        return services;
    }

    /// <summary>
    /// Adds application handlers (commands, queries, domain events, integration events) from the specified assembly.
    /// </summary>
    /// <param name="services">The <see cref="IServiceCollection"/> to add services to.</param>
    /// <param name="assembly">The assembly to scan for handlers.</param>
    /// <returns>The <see cref="IServiceCollection"/> so that additional calls can be chained.</returns>
    public static IServiceCollection AddApplicationHandlers(
        this IServiceCollection services,
        Assembly assembly)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(assembly);

        RegisterCommandAndQueryHandlers(services, assembly);
        RegisterDomainEventHandlers(services, assembly);
        RegisterIntegrationEventHandlers(services, assembly);

        return services;
    }

    /// <summary>
    /// Adds application handlers (commands, queries, domain events, integration events) from multiple assemblies.
    /// </summary>
    /// <param name="services">The <see cref="IServiceCollection"/> to add services to.</param>
    /// <param name="assemblies">The assemblies to scan for handlers.</param>
    /// <returns>The <see cref="IServiceCollection"/> so that additional calls can be chained.</returns>
    public static IServiceCollection AddApplicationHandlers(
        this IServiceCollection services,
        params Assembly[] assemblies)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(assemblies);

        if (assemblies.Length == 0)
            throw new ArgumentException("At least one assembly must be provided.", nameof(assemblies));

        foreach (var assembly in assemblies)
        {
            RegisterCommandAndQueryHandlers(services, assembly);
            RegisterDomainEventHandlers(services, assembly);
            RegisterIntegrationEventHandlers(services, assembly);
        }

        return services;
    }

    private static void RegisterCommandAndQueryHandlers(IServiceCollection services, Assembly assembly)
    {
        var handlerTypes = assembly.GetTypes()
            .Where(t => !t.IsAbstract && !t.IsInterface)
            .SelectMany(t => t.GetInterfaces()
                .Where(i => i.IsGenericType &&
                    (i.GetGenericTypeDefinition() == typeof(ICommandHandler<,>) ||
                     i.GetGenericTypeDefinition() == typeof(IQueryHandler<,>)))
                .Select(i => new { Handler = t, Interface = i }));

        foreach (var handler in handlerTypes)
        {
            services.TryAddScoped(handler.Interface, handler.Handler);
        }
    }

    private static void RegisterDomainEventHandlers(IServiceCollection services, Assembly assembly)
    {
        var domainEventHandlerTypes = assembly.GetTypes()
            .Where(t => !t.IsAbstract && !t.IsInterface)
            .Where(t => t.GetInterfaces().Any(i => i.IsGenericType &&
                i.GetGenericTypeDefinition() == typeof(IDomainEventHandler<>)));

        foreach (var handlerType in domainEventHandlerTypes)
        {
            var interfaces = handlerType.GetInterfaces()
                .Where(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IDomainEventHandler<>));

            foreach (var interfaceType in interfaces)
            {
                services.AddScoped(interfaceType, handlerType);
            }
        }
    }

    private static void RegisterIntegrationEventHandlers(IServiceCollection services, Assembly assembly)
    {
        var integrationHandlerTypes = assembly.GetTypes()
            .Where(t => !t.IsAbstract && !t.IsInterface)
            .Where(t => t.GetInterfaces().Any(i => i.IsGenericType &&
                i.GetGenericTypeDefinition() == typeof(IIntegrationEventHandler<>)));

        foreach (var handlerType in integrationHandlerTypes)
        {
            var interfaces = handlerType.GetInterfaces()
                .Where(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IIntegrationEventHandler<>));

            foreach (var interfaceType in interfaces)
            {
                services.AddScoped(interfaceType, handlerType);
            }
        }
    }
}

