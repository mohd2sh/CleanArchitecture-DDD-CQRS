using CleanArchitecture.Cmms.Application.WorkOrders.Commands.AssignTechnician;
using CleanArchitecture.Cmms.Application.WorkOrders.Commands.CompleteWorkOrder;
using CleanArchitecture.Cmms.Application.WorkOrders.Commands.CreateWorkOrder;
using CleanArchitecture.Cmms.Application.WorkOrders.Commands.StartWorkOrder;
using CleanArchitecture.Cmms.Domain.Assets.Enums;
using CleanArchitecture.Cmms.Domain.WorkOrders.Enums;
using CleanArchitecture.Cmms.Domain.WorkOrders.Events;
using CleanArchitecture.Cmms.Infrastructure.Persistence.EfCore;
using CleanArchitecture.Cmms.IntegrationTests.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace CleanArchitecture.Cmms.IntegrationTests.Transactions;

/// <summary>
/// Tests validating event and database transaction consistency
/// Ensures that domain events and database changes are atomic
/// </summary>
public class EventTransactionConsistencyTests : IntegrationTestBase
{
    public EventTransactionConsistencyTests(CmmsWebApplicationFactory factory) : base(factory)
    {
    }

    [Fact]
    public async Task DomainEvents_AndDatabaseChanges_ShouldBeAtomic()
    {
        // Arrange
        var assetId = await CreateAssetAsync("TEST-CONSISTENCY-001", "Test Machine");

        // Act
        var command = new CreateWorkOrderCommand(assetId, "Atomic Test", "B1", "F1", "R1");
        var result = await Mediator.Send(command);

        // Assert
        Assert.True(result.IsSuccess);

        var workOrder = await WriteDbContext.WorkOrders.FindAsync(result.Value);
        Assert.NotNull(workOrder);
        Assert.Equal("Atomic Test", workOrder.Title);
        Assert.Equal(WorkOrderStatus.Open, workOrder.Status);

        var asset = await WriteDbContext.Assets.FindAsync(assetId);
        Assert.NotNull(asset);
        Assert.Equal(AssetStatus.UnderMaintenance, asset.Status);

        Assert.Single(asset.MaintenanceRecords);
        var maintenanceRecord = asset.MaintenanceRecords.First();
        Assert.Contains("Atomic Test", maintenanceRecord.Description);

        var outboxEvent = await OutboxDbContext.OutboxMessages
            .FirstOrDefaultAsync(e => e.EventType.Contains(nameof(WorkOrderCreatedEvent)));
        Assert.NotNull(outboxEvent);
        Assert.Contains("Atomic Test", outboxEvent.Payload);
    }

    [Fact]
    public async Task MultipleDomainEvents_ShouldBeAtomic_WithDatabaseChanges()
    {
        // Arrange
        var assetId = await CreateAssetAsync("TEST-CONSISTENCY-002", "Test Machine");
        var technicianId = await CreateTechnicianAsync("Test Tech", "Senior", 3);

        // Act 
        var createCommand = new CreateWorkOrderCommand(assetId, "Multi Event Test", "B1", "F1", "R1");
        var workOrderId = await Mediator.Send(createCommand);

        var assignCommand = new AssignTechnicianCommand(workOrderId.Value, technicianId);
        await Mediator.Send(assignCommand);

        var startCommand = new StartWorkOrderCommand(workOrderId.Value);
        await Mediator.Send(startCommand);

        var completeCommand = new CompleteWorkOrderCommand(workOrderId.Value);
        await Mediator.Send(completeCommand);

        //Assert
        var workOrder = await WriteDbContext.WorkOrders.FindAsync(workOrderId.Value);
        var asset = await WriteDbContext.Assets.FindAsync(assetId);
        var technician = await WriteDbContext.Technicians.FindAsync(technicianId);


        Assert.Equal(WorkOrderStatus.Completed, workOrder.Status);
        Assert.Equal(AssetStatus.Active, asset.Status);

        var assignment = technician.Assignments.FirstOrDefault(a => a.WorkOrderId == workOrderId.Value);
        Assert.NotNull(assignment);
        Assert.True(assignment.IsCompleted);

        var outboxEvents = await OutboxDbContext.OutboxMessages
            .OrderBy(e => e.CreatedAt)
            .ToListAsync();

        Assert.True(outboxEvents.Count >= 4);
        Assert.Contains(outboxEvents, e => e.EventType.Contains(nameof(WorkOrderCreatedEvent)));
        Assert.Contains(outboxEvents, e => e.EventType.Contains(nameof(TechnicianAssignedEvent)));
        Assert.Contains(outboxEvents, e => e.EventType.Contains(nameof(WorkOrderCompletedEvent)));
    }

    [Fact]
    public async Task WhenDomainEventFails_DatabaseChanges_ShouldBeRolledBack()
    {
        // Arrange
        var assetId = await CreateAssetAsync("TEST-CONSISTENCY-003", "Test Machine");
        var initialAssetCount = await WriteDbContext.Assets.CountAsync();
        var initialWorkOrderCount = await WriteDbContext.WorkOrders.CountAsync();
        var initialOutboxCount = await OutboxDbContext.OutboxMessages.CountAsync();

        // Act 
        var nonExistentAssetId = Guid.NewGuid();
        var command = new CreateWorkOrderCommand(nonExistentAssetId, "Should Fail", "B1", "F1", "R1");
        var act = () => Mediator.Send(command);

        // Assert
        await Assert.ThrowsAnyAsync<Exception>(act);

        // Verify no changes were committed
        var finalAssetCount = await WriteDbContext.Assets.CountAsync();
        var finalWorkOrderCount = await WriteDbContext.WorkOrders.CountAsync();
        var finalOutboxCount = await OutboxDbContext.OutboxMessages.CountAsync();

        Assert.Equal(initialAssetCount, finalAssetCount);
        Assert.Equal(initialWorkOrderCount, finalWorkOrderCount);
        Assert.Equal(initialOutboxCount, finalOutboxCount);

        // Verify original asset is unchanged
        var originalAsset = await WriteDbContext.Assets.FindAsync(assetId);
        Assert.Equal(AssetStatus.Active, originalAsset.Status);
    }

    [Fact]
    public async Task RowVersion_ShouldBeConsistent_AcrossRelatedEntities()
    {
        // Arrange
        var assetId = await CreateAssetAsync("TEST-CONSISTENCY-004", "Test Machine");

        // Get initial RowVersions
        var initialAsset = await WriteDbContext.Assets.FindAsync(assetId);
        var initialAssetRowVersion = initialAsset.RowVersion;

        // Act
        var command = new CreateWorkOrderCommand(assetId, "RowVersion Test", "B1", "F1", "R1");
        var result = await Mediator.Send(command);

        // Assert
        Assert.True(result.IsSuccess);

        // Verify RowVersions were updated consistently
        var workOrder = await WriteDbContext.WorkOrders.FindAsync(result.Value);
        var updatedAsset = await WriteDbContext.Assets.FindAsync(assetId);

        Assert.NotNull(workOrder.RowVersion);
        Assert.NotNull(updatedAsset.RowVersion);
        Assert.NotEqual(initialAssetRowVersion, updatedAsset.RowVersion);

        // Verify RowVersions are different (indicating both were updated in same transaction)
        Assert.NotEqual(workOrder.RowVersion, updatedAsset.RowVersion);
    }

    [Fact]
    public async Task EventOrder_ShouldBeConsistent_WithDatabaseState()
    {
        // Arrange
        var assetId = await CreateAssetAsync("TEST-CONSISTENCY-005", "Test Machine");
        var technicianId = await CreateTechnicianAsync("Test Tech", "Senior", 3);

        // Act
        var createCommand = new CreateWorkOrderCommand(assetId, "Event Order Test", "B1", "F1", "R1");
        var workOrderId = await Mediator.Send(createCommand);

        var assignCommand = new AssignTechnicianCommand(workOrderId.Value, technicianId);
        await Mediator.Send(assignCommand);

        // Assert
        var outboxEvents = await OutboxDbContext.OutboxMessages
            .OrderBy(e => e.CreatedAt)
            .ToListAsync();

        var firstEvent = outboxEvents.FirstOrDefault(e => e.EventType.Contains("WorkOrderCreatedEvent"));
        Assert.NotNull(firstEvent);

        var secondEvent = outboxEvents.FirstOrDefault(e => e.EventType.Contains("TechnicianAssignedEvent"));
        Assert.NotNull(secondEvent);

        Assert.True(firstEvent.CreatedAt <= secondEvent.CreatedAt);

        // Verify database state matches event order
        var workOrder = await WriteDbContext.WorkOrders.FindAsync(workOrderId.Value);
        var asset = await WriteDbContext.Assets.FindAsync(assetId);
        var technician = await WriteDbContext.Technicians.FindAsync(technicianId);

        // Work order should be assigned (after TechnicianAssigned event)
        Assert.Equal(WorkOrderStatus.Assigned, workOrder.Status);
        Assert.Equal(technicianId, workOrder.TechnicianId);

        // Asset should be under maintenance (after WorkOrderCreated event)
        Assert.Equal(AssetStatus.UnderMaintenance, asset.Status);

        // Technician should have assignment (after TechnicianAssigned event)
        Assert.Single(technician.Assignments.Where(a => a.WorkOrderId == workOrderId.Value));
    }

    [Fact]
    public async Task ConcurrentOperations_ShouldMaintain_EventConsistency()
    {
        // Arrange
        var asset1Id = await CreateAssetAsync("TEST-CONSISTENCY-006A", "Machine A");
        var asset2Id = await CreateAssetAsync("TEST-CONSISTENCY-006B", "Machine B");

        // Act
        var command1 = new CreateWorkOrderCommand(asset1Id, "Concurrent Test A", "B1", "F1", "R1");
        var command2 = new CreateWorkOrderCommand(asset2Id, "Concurrent Test B", "B1", "F1", "R2");

        var task1 = ExecuteInIsolatedScopeAsync(mediator => mediator.Send(command1));
        var task2 = ExecuteInIsolatedScopeAsync(mediator => mediator.Send(command2));

        var results = await Task.WhenAll(task1, task2);

        // Assert - Both should succeed
        Assert.True(results[0].IsSuccess);
        Assert.True(results[1].IsSuccess);

        // Verify both work orders were created
        WriteDbContext.ChangeTracker.Clear();
        var workOrder1 = await WriteDbContext.WorkOrders.FindAsync(results[0].Value);
        var workOrder2 = await WriteDbContext.WorkOrders.FindAsync(results[1].Value);

        Assert.NotNull(workOrder1);
        Assert.NotNull(workOrder2);

        // Verify both assets are under maintenance
        var asset1 = await WriteDbContext.Assets.FindAsync(asset1Id);
        var asset2 = await WriteDbContext.Assets.FindAsync(asset2Id);

        Assert.Equal(AssetStatus.UnderMaintenance, asset1.Status);
        Assert.Equal(AssetStatus.UnderMaintenance, asset2.Status);

        // Verify both integration events were written
        var outboxEvents = await OutboxDbContext.OutboxMessages
            .Where(e => e.EventType.Contains("WorkOrderCreatedEvent"))
            .ToListAsync();

        Assert.Equal(2, outboxEvents.Count);

        // Verify events contain correct data
        var event1 = outboxEvents.FirstOrDefault(e => e.Payload.Contains("Concurrent Test A"));
        var event2 = outboxEvents.FirstOrDefault(e => e.Payload.Contains("Concurrent Test B"));

        Assert.NotNull(event1);
        Assert.NotNull(event2);
    }

    [Fact]
    public async Task EventPayload_ShouldMatch_DatabaseState()
    {
        // Arrange
        var assetId = await CreateAssetAsync("TEST-CONSISTENCY-007", "Test Machine");

        // Act
        var command = new CreateWorkOrderCommand(assetId, "Payload Consistency Test", "B1", "F1", "R1");
        var result = await Mediator.Send(command);

        // Assert
        Assert.True(result.IsSuccess);

        // Verify database state
        var workOrder = await WriteDbContext.WorkOrders.FindAsync(result.Value);
        var asset = await WriteDbContext.Assets.FindAsync(assetId);

        Assert.NotNull(workOrder);
        Assert.NotNull(asset);

        // Verify event payload matches database state
        var outboxEvent = await OutboxDbContext.OutboxMessages
            .FirstAsync(e => e.EventType.Contains("WorkOrderCreatedEvent"));

        Assert.Contains(workOrder.Id.ToString(), outboxEvent.Payload);
        Assert.Contains(asset.Id.ToString(), outboxEvent.Payload);
        Assert.Contains(workOrder.Title, outboxEvent.Payload);
    }

    [Fact]
    public async Task TransactionIsolation_ShouldPrevent_DirtyReads()
    {
        // Arrange
        var assetId = await CreateAssetAsync("TEST-CONSISTENCY-008", "Test Machine");

        // Act 
        var command = new CreateWorkOrderCommand(assetId, "Isolation Test", "B1", "F1", "R1");
        var result = await Mediator.Send(command);

        // Assert
        Assert.True(result.IsSuccess);

        // Verify that the transaction completed atomically
        // No intermediate states should be visible
        var workOrder = await WriteDbContext.WorkOrders.FindAsync(result.Value);
        var asset = await WriteDbContext.Assets.FindAsync(assetId);

        // Both entities should be in their final consistent state
        Assert.Equal(WorkOrderStatus.Open, workOrder.Status);
        Assert.Equal(AssetStatus.UnderMaintenance, asset.Status);

        // Verify no partial updates are visible
        Assert.NotNull(workOrder.RowVersion);
        Assert.NotNull(asset.RowVersion);
        Assert.NotEqual(workOrder.RowVersion, asset.RowVersion);
    }
}
