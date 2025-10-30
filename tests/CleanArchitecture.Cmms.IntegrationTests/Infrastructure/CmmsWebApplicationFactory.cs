using System.Data;
using CleanArchitecture.Cmms.Api;
using CleanArchitecture.Cmms.Infrastructure.Persistence.EfCore;
using CleanArchitecture.Outbox.Persistence;
using DotNet.Testcontainers.Builders;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Testcontainers.MsSql;

namespace CleanArchitecture.Cmms.IntegrationTests.Infrastructure;

public sealed class CmmsWebApplicationFactory : WebApplicationFactory<Program>, IAsyncLifetime
{

    private readonly MsSqlContainer _sqlServerContainer;

    public CmmsWebApplicationFactory()
    {
        _sqlServerContainer = new MsSqlBuilder()
        .WithImage("mcr.microsoft.com/mssql/server:2022-latest")
        .WithPassword("YourStrong!Passw0rd")
        .WithWaitStrategy(Wait.ForUnixContainer().UntilDatabaseIsAvailable(SqlClientFactory.Instance))
        .WithEnvironment("ACCEPT_EULA", "Y")
        .WithCleanUp(true)
        .Build();
    }
    public string ConnectionString => _sqlServerContainer.GetConnectionString() /*+ ";TrustServerCertificate=True"*/;

    public async Task InitializeAsync()
    {
        await _sqlServerContainer.StartAsync();

        var options = new DbContextOptionsBuilder<WriteDbContext>()
            .UseSqlServer(ConnectionString)
            .Options;

        var outBoxOptions = new DbContextOptionsBuilder<OutboxDbContext>()
            .UseSqlServer(ConnectionString)
            .Options;

        // Ensure EF Core migrations are applied once per test run
        using var scope = Services.CreateScope();
        using var writeDb = new WriteDbContext(options);
        using var outboxDb = new OutboxDbContext(outBoxOptions);

        await writeDb.Database.EnsureCreatedAsync();
        await outboxDb.Database.MigrateAsync();

    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");

        builder.ConfigureServices(services =>
        {
            RemoveDbContext<WriteDbContext>(services);
            RemoveDbContext<ReadDbContext>(services);
            RemoveDbContext<OutboxDbContext>(services);
            RemoveService<IDbConnection>(services);
            RemoveService<SqlConnection>(services);

            var connectionString = ConnectionString;

            services.AddDbContext<WriteDbContext>(o =>
                o.UseSqlServer(connectionString).EnableSensitiveDataLogging());

            services.AddDbContext<ReadDbContext>(o =>
                o.UseSqlServer(connectionString)
                 .UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking)
                 .EnableSensitiveDataLogging());

            services.AddDbContext<OutboxDbContext>(o =>
                o.UseSqlServer(connectionString)
                 .EnableSensitiveDataLogging());

            services.AddTransient<IDbConnection>(_ => new SqlConnection(connectionString));

            RemoveHostedService<CleanArchitecture.Outbox.Processing.OutboxProcessor>(services);
        });
    }

    private static void RemoveService<T>(IServiceCollection services)
    {
        var descriptors = services.Where(d => d.ServiceType == typeof(T)).ToList();
        foreach (var descriptor in descriptors)
        {
            services.Remove(descriptor);
        }
    }

    private static void RemoveDbContext<T>(IServiceCollection services) where T : DbContext
    {
        var descriptors = services.Where(d =>
            d.ServiceType == typeof(T) ||
            d.ServiceType == typeof(DbContextOptions<T>) ||
            d.ServiceType == typeof(DbContextOptions)).ToList();
        foreach (var descriptor in descriptors)
        {
            services.Remove(descriptor);
        }
    }

    private static void RemoveHostedService<THosted>(IServiceCollection services)
    {
        var descriptors = services
            .Where(d => d.ServiceType == typeof(IHostedService)
                && d.ImplementationType == typeof(THosted))
            .ToList();

        foreach (var descriptor in descriptors)
        {
            services.Remove(descriptor);
        }
    }

    public new async Task DisposeAsync()
    {
        await _sqlServerContainer.DisposeAsync();
        await base.DisposeAsync();
    }
}
