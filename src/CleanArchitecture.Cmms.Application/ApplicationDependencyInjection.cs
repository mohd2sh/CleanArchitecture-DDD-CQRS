using CleanArchitecture.Cmms.Application.Behaviors;
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;

namespace CleanArchitecture.Cmms.Application;

public static class ApplicationDependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        var assembly = typeof(ApplicationDependencyInjection).Assembly;

        // Register Command & Query Handlers
        AddCommandAndQueryHandlers(services, assembly);

        AddCommandPipelines(services);

        services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(ApplicationDependencyInjection).Assembly));

        services.AddScoped<IMediator, MediatRAdapter>();

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
        services.AddTransient(typeof(MediatR.IPipelineBehavior<,>), typeof(TestQueryPipeline<,>));
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
