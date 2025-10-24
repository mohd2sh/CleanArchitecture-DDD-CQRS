using CleanArchitecture.Cmms.Outbox.Abstractions;
using CleanArchitecture.Cmms.Outbox.Persistence;
using CleanArchitecture.Cmms.Outbox.Processing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace CleanArchitecture.Cmms.Outbox;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddOutbox(
        this IServiceCollection services,
        string connectionString)
    {
        // Register OutboxDbContext
        services.AddDbContext<OutboxDbContext>(options =>
            options.UseSqlServer(connectionString,
                sql => sql.MigrationsAssembly(typeof(OutboxDbContext).Assembly.FullName)));

        // Register outbox store
        services.AddScoped<IOutboxStore, EfCoreOutboxStore>();

        // Register background processor
        services.AddHostedService<OutboxProcessor>();

        return services;
    }
}
