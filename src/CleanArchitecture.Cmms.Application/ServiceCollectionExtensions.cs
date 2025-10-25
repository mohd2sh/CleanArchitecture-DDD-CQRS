using System.Reflection;
using CleanArchitecture.Cmms.Application.Behaviors;
using CleanArchitecture.Cmms.Application.ErrorManagement;
using CleanArchitecture.Cmms.Application.Integrations.Events.WorkOrderCompleted;
using CleanArchitecture.Core.Application.Abstractions.Events;
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;

namespace CleanArchitecture.Cmms.Application;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        var assembly = typeof(ServiceCollectionExtensions).Assembly;

        // Register Command & Query Handlers
        AddCommandAndQueryHandlers(services, assembly);

        // Register Domain Event Handlers
        AddDomainEventHandlers(services, assembly);

        // Register Integration Event Handlers
        AddIntegrationEventHandlers(services, assembly);

        AddPipelines(services);

        //Fluent Validation
        services.AddValidatorsFromAssembly(assembly, includeInternalTypes: true);

        services.AddSingleton<IErrorExporter, ErrorExporter>();

        // Register mock email service
        services.AddScoped<IEmailService, MockEmailService>();
        return services;
    }

    private static void AddPipelines(IServiceCollection services)
    {
        // Generic pipeline - runs for both commands and queries
        services.AddScoped(typeof(IPipeline<,>), typeof(LoggingPipeline<,>));
        services.AddScoped(typeof(IPipeline<,>), typeof(ValidationPipeline<,>));

        // Command-specific pipelines - run only for commands
        services.AddScoped(typeof(ICommandPipeline<,>), typeof(TransactionCommandPipeline<,>));
        services.AddScoped(typeof(ICommandPipeline<,>), typeof(DomainEventsPipeline<,>));
    }

    private static void AddCommandAndQueryHandlers(IServiceCollection services, Assembly assembly)
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
            services.AddScoped(handler.Interface, handler.Handler);
        }
    }

    private static void AddDomainEventHandlers(IServiceCollection services, Assembly assembly)
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

    private static void AddIntegrationEventHandlers(IServiceCollection services, Assembly assembly)
    {
        // Register integration event handlers manually
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
