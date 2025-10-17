using CleanArchitecture.Cmms.Application.Abstractions.Common;
using CleanArchitecture.Cmms.Application.Abstractions.Persistence;
using CleanArchitecture.Cmms.Application.Abstractions.Persistence.Repositories;
using CleanArchitecture.Cmms.Application.WorkOrders.Interfaces;
using CleanArchitecture.Cmms.Infrastructure.Common;
using CleanArchitecture.Cmms.Infrastructure.Persistence.EfCore;
using CleanArchitecture.Cmms.Infrastructure.Persistence.EfCore.Interceptors;
using CleanArchitecture.Cmms.Infrastructure.Repositories.ReadRepositories;
using CleanArchitecture.Cmms.Infrastructure.Repositories.ReadRepositories.WorkOrders;
using CleanArchitecture.Cmms.Infrastructure.Repositories.WriteRepositories;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.Data;

namespace CleanArchitecture.Cmms.Infrastructure;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration config, string environment)
    {
        AddWriteDbServices(services, config);

        AddReadDbServices(services, config, environment);

        services.AddSingleton<IDateTimeProvider, SystemDateTimeProvider>();

        return services;
    }


    private static void AddWriteDbServices(IServiceCollection services, IConfiguration config)
    {
        services.AddScoped<AuditableEntityInterceptor>();

        services.AddDbContext<WriteDbContext>((sp, opt) =>
        {
            var interceptor = sp.GetRequiredService<AuditableEntityInterceptor>();

            opt.AddInterceptors(interceptor);

            opt.UseSqlServer(
                config.GetConnectionString("WriteDb"),
                sql => sql.MigrationsAssembly(typeof(WriteDbContext).Assembly.FullName));
        }
        );


        services.AddDbContext<WriteDbContext>(opt => opt.UseSqlServer(config.GetConnectionString("WriteDb")));

        services.AddScoped(typeof(IRepository<,>), typeof(EfRepository<,>));

        services.AddScoped<IUnitOfWork, EfUnitOfWork>();
    }

    private static void AddReadDbServices(IServiceCollection services, IConfiguration config, string environment)
    {
        //For queries we used IReadRepo using Dapper + Ef ReadDbContext
        services.AddDbContext<ReadDbContext>(opt =>
        {
            opt.UseSqlServer(
                config.GetConnectionString("ReadDb") ?? config.GetConnectionString("WriteDb"),
                sql => sql.MigrationsAssembly(typeof(ReadDbContext).Assembly.FullName));

            if (environment == "Development")
                opt.LogTo(Console.WriteLine, LogLevel.Information);
        });

        services.AddScoped(typeof(IReadRepository<,>), typeof(EfReadRepository<,>));

        services.AddTransient<IDbConnection>(sp =>
        {
            var connectionString = config.GetConnectionString("ReadDb")
                ?? config.GetConnectionString("WriteDb");
            return new SqlConnection(connectionString);
        });
        services.AddScoped<IWorkOrderReadRepository, WorkOrderReadRepository>();
    }

}
