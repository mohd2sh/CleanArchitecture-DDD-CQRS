using System;
using CleanArchitecture.Outbox.Abstractions;
using CleanArchitecture.Outbox.Persistence;
using CleanArchitecture.Outbox.Processing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace CleanArchitecture.Outbox;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddOutbox(
        this IServiceCollection services,
        string connectionString,
        OutboxOptions? options = null)
    {
        options ??= new OutboxOptions();

        // Register OutboxDbContext
        services.AddDbContext<OutboxDbContext>(dbOptions =>
            dbOptions.UseSqlServer(connectionString,
                sql => sql.MigrationsAssembly(typeof(OutboxDbContext).Assembly.FullName)));

        // Register outbox store
        services.AddScoped<IOutboxStore, EfCoreOutboxStore>();
        services.AddScoped<ICustomOutboxStore, EfCoreOutboxStore>();

        // Register processor implementation
        services.AddScoped<IOutboxProcessor, OutboxMessageProcessor>();

        // Register multiple worker instances for parallel processing
        for (int i = 0; i < options.WorkerCount; i++)
        {
            var workerId = i + 1;
            services.AddSingleton<IHostedService>(sp => new OutboxProcessor(
                sp.GetRequiredService<IServiceScopeFactory>(),
                sp.GetRequiredService<Microsoft.Extensions.Logging.ILogger<OutboxProcessor>>(),
                workerId));
        }

        return services;
    }
}
