using CleanArchitecture.Core.Application.Abstractions.Events;
using CleanArchitecture.Outbox.Persistence;
using CleanArchitecture.Outbox.Processing;
using DotNet.Testcontainers.Builders;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Testcontainers.MsSql;

namespace CleanArchitecture.Outbox.IntegrationTests.Infrastructure;

public sealed class OutboxWebApplicationFactory : IAsyncLifetime, IDisposable
{
    private readonly MsSqlContainer _sqlServerContainer;

    public OutboxWebApplicationFactory()
    {
        _sqlServerContainer = new MsSqlBuilder()
            .WithImage("mcr.microsoft.com/mssql/server:2022-latest")
            .WithPassword("YourStrong!Passw0rd")
            .WithWaitStrategy(Wait.ForUnixContainer().UntilDatabaseIsAvailable(SqlClientFactory.Instance))
            .WithEnvironment("ACCEPT_EULA", "Y")
            .WithCleanUp(true)
            .Build();
    }

    public string ConnectionString => _sqlServerContainer.GetConnectionString();

    public IServiceProvider Services { get; private set; } = null!;

    public async Task InitializeAsync()
    {
        await _sqlServerContainer.StartAsync();

        var services = new ServiceCollection();

        // Configure OutboxDbContext
        services.AddDbContext<OutboxDbContext>(options =>
            options.UseSqlServer(ConnectionString));

        // Register outbox services
        services.AddScoped<ICustomOutboxStore, EfCoreOutboxStore>();
        services.AddScoped<IOutboxProcessor, OutboxMessageProcessor>();

        // Register integration event convention (required for dispatcher)
        services.AddSingleton<IIntegrationEventConvention, TestIntegrationEventConvention>();

        // Register integration event dispatcher (test implementation)
        services.AddScoped<IIntegrationEventDispatcher, TestIntegrationEventDispatcher>();

        // Register logging
        services.AddLogging();

        // Register test handler for TestDomainEvent
        services.AddScoped<IIntegrationEventHandler<TestDomainEvent>, TestIntegrationEventHandler<TestDomainEvent>>();

        // Ensure migrations are applied
        var options = new DbContextOptionsBuilder<OutboxDbContext>()
            .UseSqlServer(ConnectionString)
            .Options;

        using var context = new OutboxDbContext(options);
        await context.Database.MigrateAsync();

        Services = services.BuildServiceProvider();
    }

    public async Task DisposeAsync()
    {
        await _sqlServerContainer.DisposeAsync();
    }

    public void Dispose()
    {
        DisposeAsync().GetAwaiter().GetResult();
    }
}

