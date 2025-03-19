using CleanArchitecture.Cmms.Application.Abstractions.Persistence;
using CleanArchitecture.Cmms.Application.WorkOrders.Interfaces;
using CleanArchitecture.Cmms.Infrastructure.Persistence;
using CleanArchitecture.Cmms.Infrastructure.ReadRepositories.WorkOrders;
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
