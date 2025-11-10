using CleanArchitecture.Core.Application.Abstractions.Events;
using CleanArchitecture.Core.Application.Abstractions.Messaging;
using CleanArchitecture.Core.Infrastructure.Messaging;
using CleanArchitecture.Core.Infrastructure.Messaging.Conventions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace CleanArchitecture.Core.Infrastructure;

/// <summary>
/// Extension methods for configuring Core Infrastructure services.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Adds Core Infrastructure services (Mediator, Domain Event Dispatcher, Integration Event Dispatcher)
    /// with default configuration.
    /// </summary>
    /// <param name="services">The <see cref="IServiceCollection"/> to add services to.</param>
    /// <returns>The <see cref="IServiceCollection"/> so that additional calls can be chained.</returns>
    public static IServiceCollection AddCoreInfrastructure(this IServiceCollection services)
    {
        return services.AddCoreInfrastructure(_ => { });
    }

    /// <summary>
    /// Adds Core Infrastructure services (Mediator, Domain Event Dispatcher, Integration Event Dispatcher)
    /// with optional configuration.
    /// </summary>
    /// <param name="services">The <see cref="IServiceCollection"/> to add services to.</param>
    /// <param name="configure">An action to configure the <see cref="CoreInfrastructureOptions"/>.</param>
    /// <returns>The <see cref="IServiceCollection"/> so that additional calls can be chained.</returns>
    public static IServiceCollection AddCoreInfrastructure(
        this IServiceCollection services,
        Action<CoreInfrastructureOptions> configure)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(configure);

        var options = new CoreInfrastructureOptions();
        configure(options);

        // Register Mediator
        services.TryAddScoped<IMediator, CustomMediator>();

        // Register Domain Event Dispatcher
        services.TryAddScoped<IDomainEventDispatcher, DomainEventDispatcher>();

        // Register Integration Event Convention
        if (options.CustomConvention != null)
        {
            services.TryAddSingleton<IIntegrationEventConvention>(options.CustomConvention);
        }
        else if (options.RegisterConvention)
        {
            services.TryAddSingleton<IIntegrationEventConvention, DefaultIntegrationEventConvention>();
        }

        // Register Integration Event Dispatcher
        services.TryAddScoped<IIntegrationEventDispatcher, IntegrationEventDispatcher>();

        return services;
    }

    /// <summary>
    /// Adds the custom mediator implementation.
    /// </summary>
    /// <param name="services">The <see cref="IServiceCollection"/> to add services to.</param>
    /// <returns>The <see cref="IServiceCollection"/> so that additional calls can be chained.</returns>
    public static IServiceCollection AddMediator(this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);
        services.TryAddScoped<IMediator, CustomMediator>();
        return services;
    }

    /// <summary>
    /// Adds the domain event dispatcher implementation.
    /// </summary>
    /// <param name="services">The <see cref="IServiceCollection"/> to add services to.</param>
    /// <returns>The <see cref="IServiceCollection"/> so that additional calls can be chained.</returns>
    public static IServiceCollection AddDomainEventDispatcher(this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);
        services.TryAddScoped<IDomainEventDispatcher, DomainEventDispatcher>();
        return services;
    }

    /// <summary>
    /// Adds the integration event dispatcher and convention with optional configuration.
    /// </summary>
    /// <param name="services">The <see cref="IServiceCollection"/> to add services to.</param>
    /// <param name="configure">An optional action to configure the integration event options.</param>
    /// <returns>The <see cref="IServiceCollection"/> so that additional calls can be chained.</returns>
    public static IServiceCollection AddIntegrationEventDispatcher(
        this IServiceCollection services,
        Action<CoreInfrastructureOptions>? configure = null)
    {
        ArgumentNullException.ThrowIfNull(services);

        var options = new CoreInfrastructureOptions();
        configure?.Invoke(options);

        // Register Integration Event Convention
        if (options.CustomConvention != null)
        {
            services.TryAddSingleton<IIntegrationEventConvention>(options.CustomConvention);
        }
        else if (options.RegisterConvention)
        {
            services.TryAddSingleton<IIntegrationEventConvention, DefaultIntegrationEventConvention>();
        }

        // Register Integration Event Dispatcher
        services.TryAddScoped<IIntegrationEventDispatcher, IntegrationEventDispatcher>();

        return services;
    }
}

