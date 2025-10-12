using CleanArchitecture.Cmms.Application.Abstractions.Persistence;
using CleanArchitecture.Cmms.Application.Abstractions.Persistence.Repositories;
using CleanArchitecture.Cmms.Application.WorkOrders.Interfaces;
using CleanArchitecture.Cmms.Infrastructure.Persistence.EfCore;
using CleanArchitecture.Cmms.Infrastructure.Repositories.ReadRepositories;
using CleanArchitecture.Cmms.Infrastructure.Repositories.ReadRepositories.WorkOrders;
using CleanArchitecture.Cmms.Infrastructure.Repositories.WriteRepositories;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System.Data;

namespace CleanArchitecture.Cmms.Infrastructure;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration config)
    {
        services.AddDbContext<WriteDbContext>(opt =>
          opt.UseSqlServer(
              config.GetConnectionString("WriteDb"),
              sql => sql.MigrationsAssembly(typeof(WriteDbContext).Assembly.FullName)));


        services.AddDbContext<WriteDbContext>(opt => opt.UseSqlServer(config.GetConnectionString("WriteDb")));
        services.AddScoped(typeof(IRepository<,>), typeof(EfRepository<,>));
        services.AddScoped<IUnitOfWork, EfUnitOfWork>();

        //For queries we used IReadRepo using Dapper + Ef ReadDbContext
        services.AddDbContext<ReadDbContext>(opt =>
            opt.UseSqlServer(
                config.GetConnectionString("ReadDb") ?? config.GetConnectionString("WriteDb"),
                sql => sql.MigrationsAssembly(typeof(ReadDbContext).Assembly.FullName)));

        services.AddScoped(typeof(IReadRepository<,>), typeof(EfReadRepository<,>));

        services.AddTransient<IDbConnection>(sp =>
        {
            var connectionString = config.GetConnectionString("ReadDb")
                ?? config.GetConnectionString("WriteDb");
            return new SqlConnection(connectionString);
        });
        services.AddScoped<IWorkOrderReadRepository, WorkOrderReadRepository>();
        return services;
    }
}
