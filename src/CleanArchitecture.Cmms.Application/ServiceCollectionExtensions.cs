using System.Reflection;
using CleanArchitecture.Cmms.Application.Behaviors;
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

        AddCommandPipelines(services);

        services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(ServiceCollectionExtensions).Assembly));

        //Fluent Validation
        services.AddValidatorsFromAssembly(assembly, includeInternalTypes: true);

        return services;
    }

    private static void AddCommandPipelines(IServiceCollection services)
    {
        services.AddTransient(typeof(MediatR.IPipelineBehavior<,>), typeof(LoggingPipeline<,>));
        services.AddTransient(typeof(MediatR.IPipelineBehavior<,>), typeof(ValidationPipeline<,>));
        services.AddTransient(typeof(MediatR.IPipelineBehavior<,>), typeof(TransactionCommandPipeline<,>));
        services.AddTransient(typeof(MediatR.IPipelineBehavior<,>), typeof(DomainEventsPipeline<,>));
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

        foreach (var h in handlerTypes)
        {
            services.AddTransient(h.Interface, h.Handler);
        }
    }
}
