using CleanArchitecture.Cmms.Application.Assets.Commands.CreateAsset;
using CleanArchitecture.Cmms.Application.Technicians.Commands.CreateTechnician;
using CleanArchitecture.Cmms.Application.WorkOrders.Commands.CreateWorkOrder;
using CleanArchitecture.Cmms.Infrastructure.Persistence.EfCore;
using CleanArchitecture.Core.Application.Abstractions.Messaging;
using CleanArchitecture.Outbox.Persistence;
using Microsoft.Data.SqlClient;
using Respawn;
namespace CleanArchitecture.Cmms.IntegrationTests.Infrastructure;

public abstract class IntegrationTestBase : IClassFixture<CmmsWebApplicationFactory>, IAsyncLifetime
{
    private static Respawner _respawner;
    private static readonly SemaphoreSlim _respawnerLock = new(1, 1);
    private readonly CmmsWebApplicationFactory _factory;
    protected HttpClient Client { get; private set; }
    protected IServiceScope Scope { get; private set; } = null!;
    protected WriteDbContext WriteDbContext { get; private set; } = null!;
    protected ReadDbContext ReadDbContext { get; private set; } = null!;
    protected OutboxDbContext OutboxDbContext { get; private set; } = null!;
    protected IMediator Mediator { get; private set; } = null!;

    protected IntegrationTestBase(CmmsWebApplicationFactory factory)
    {
        _factory = factory;
    }

    public async Task InitializeAsync()
    {
        await DbRespawner();

        Client = _factory.CreateClient();
        Scope = _factory.Services.CreateScope();
        WriteDbContext = Scope.ServiceProvider.GetRequiredService<WriteDbContext>();
        ReadDbContext = Scope.ServiceProvider.GetRequiredService<ReadDbContext>();
        OutboxDbContext = Scope.ServiceProvider.GetRequiredService<OutboxDbContext>();
        Mediator = Scope.ServiceProvider.GetRequiredService<IMediator>();
    }

    private async Task DbRespawner()
    {
        if (_respawner == null)
        {
            await _respawnerLock.WaitAsync();
            try
            {
                if (_respawner == null)
                {
                    await using var conn = new SqlConnection(_factory.ConnectionString);
                    await conn.OpenAsync();
                    _respawner = await Respawner.CreateAsync(conn, new RespawnerOptions
                    {
                        DbAdapter = DbAdapter.SqlServer,
                        TablesToIgnore = ["__EFMigrationsHistory"]
                    });
                }
            }
            finally
            {
                _respawnerLock.Release();
            }
        }

        await using (var conn = new SqlConnection(_factory.ConnectionString))
        {
            await conn.OpenAsync();
            await _respawner.ResetAsync(conn);
        }
    }

    public async Task DisposeAsync()
    {
        // Let container lifecycle handle cleanup; avoid drop/create each test.
        Scope?.Dispose();
        await Task.CompletedTask;
    }

    protected async Task<Guid> CreateAssetAsync(string tag = "TEST-001", string name = "Test Asset", string type = "Equipment", string site = "Main Site", string area = "Production", string zone = "Zone A")
    {
        var command = new CreateAssetCommand(name, type, tag, site, area, zone);
        var result = await Mediator.Send(command);
        return result.Value;
    }

    protected async Task<Guid> CreateTechnicianAsync(string name = "John Doe", string skillLevelName = "Senior", int skillLevelRank = 3)
    {
        var command = new CreateTechnicianCommand(name, skillLevelName, skillLevelRank);
        var result = await Mediator.Send(command);
        return result.Value;
    }

    protected async Task<Guid> CreateWorkOrderAsync(Guid assetId, string title = "Test Work Order", string building = "Building A", string floor = "Floor 1", string room = "Room 101")
    {
        var command = new CreateWorkOrderCommand(assetId, title, building, floor, room);
        var result = await Mediator.Send(command);
        return result.Value;
    }

    protected async Task<TResult> ExecuteInIsolatedScopeAsync<TResult>(
        Func<IMediator, Task<TResult>> action)
    {
        using var scope = _factory.Services.CreateScope();
        var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();
        return await action(mediator);
    }

    protected async Task ExecuteInIsolatedScopeAsync(
        Func<IMediator, Task> action)
    {
        using var scope = _factory.Services.CreateScope();
        var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();
        await action(mediator);
    }
}
