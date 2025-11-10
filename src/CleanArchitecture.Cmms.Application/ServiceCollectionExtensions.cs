using CleanArchitecture.Cmms.Application.Behaviors;
using CleanArchitecture.Cmms.Application.ErrorManagement;
using CleanArchitecture.Cmms.Application.Integrations.Events.WorkOrderCompleted;
using CleanArchitecture.Core.Application;
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;

namespace CleanArchitecture.Cmms.Application;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        var assembly = typeof(ServiceCollectionExtensions).Assembly;

        // Register Command & Query Handlers, Domain Event Handlers, and Integration Event Handlers
        services.AddApplicationHandlers(assembly);

        // Register Pipelines (order matters, so user controls registration)
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
}
