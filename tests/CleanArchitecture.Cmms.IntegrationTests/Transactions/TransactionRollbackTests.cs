using CleanArchitecture.Cmms.Application.Assets.Commands.CreateAsset;
using CleanArchitecture.Cmms.Application.Technicians.Commands.CreateTechnician;
using CleanArchitecture.Cmms.Application.WorkOrders.Commands.AssignTechnician;
using CleanArchitecture.Cmms.Application.WorkOrders.Commands.CompleteWorkOrder;
using CleanArchitecture.Cmms.Application.WorkOrders.Commands.CreateWorkOrder;
using CleanArchitecture.Cmms.Domain.Assets.Enums;
using CleanArchitecture.Cmms.Domain.WorkOrders.Enums;
using CleanArchitecture.Cmms.Infrastructure.Persistence.EfCore;
using CleanArchitecture.Cmms.IntegrationTests.Infrastructure;
using CleanArchitecture.Core.Domain.Abstractions;
using Microsoft.EntityFrameworkCore;

namespace CleanArchitecture.Cmms.IntegrationTests.Transactions;

/// <summary>
/// Tests validating transaction rollback scenarios and consistency
/// Ensures that failures properly rollback all changes within a transaction
/// </summary>
public class TransactionRollbackTests : IntegrationTestBase
{
    public TransactionRollbackTests(CmmsWebApplicationFactory factory) : base(factory)
    {
    }

    [Fact]
    public async Task WhenValidationFails_ShouldNotCommit_AnyChanges()
    {
        // Arrange
        var assetId = await CreateAssetAsync("TEST-ROLLBACK-001", "Test Machine");
        var initialAssetCount = await WriteDbContext.Assets.CountAsync();
        var initialWorkOrderCount = await WriteDbContext.WorkOrders.CountAsync();
        var initialOutboxCount = await OutboxDbContext.OutboxMessages.CountAsync();
        var command = new CreateWorkOrderCommand(assetId, "", "", "", "");

        // Act
        var act = () => Mediator.Send(command);

        // Assert
        await Assert.ThrowsAnyAsync<Exception>(act);

        var finalAssetCount = await WriteDbContext.Assets.CountAsync();
        var finalWorkOrderCount = await WriteDbContext.WorkOrders.CountAsync();
        var finalOutboxCount = await OutboxDbContext.OutboxMessages.CountAsync();

        Assert.Equal(initialAssetCount, finalAssetCount);
        Assert.Equal(initialWorkOrderCount, finalWorkOrderCount);
        Assert.Equal(initialOutboxCount, finalOutboxCount);

        var asset = await WriteDbContext.Assets.FindAsync(assetId);
        Assert.NotNull(asset);
        Assert.Equal(AssetStatus.Active, asset.Status); // Should remain Active, not UnderMaintenance
    }

    [Fact]
    public async Task WhenDomainEventHandlerFails_ShouldRollback_EntireTransaction()
    {
        // Arrange
        await CreateAssetAsync("TEST-ROLLBACK-002", "Test Machine");
        var initialAssetCount = await WriteDbContext.Assets.CountAsync();
        var initialWorkOrderCount = await WriteDbContext.WorkOrders.CountAsync();
        var initialOutboxCount = await OutboxDbContext.OutboxMessages.CountAsync();

        // Act - Try to create work order for non-existent asset (should cause domain event handler to fail)
        var nonExistentAssetId = Guid.NewGuid();
        var command = new CreateWorkOrderCommand(nonExistentAssetId, "Test Work Order", "B1", "F1", "R1");
        var act = () => Mediator.Send(command);

        // Assert
        await Assert.ThrowsAsync<Core.Application.Abstractions.Common.ApplicationException>(act);

        var finalAssetCount = await WriteDbContext.Assets.CountAsync();
        var finalWorkOrderCount = await WriteDbContext.WorkOrders.CountAsync();
        var finalOutboxCount = await OutboxDbContext.OutboxMessages.CountAsync();

        Assert.Equal(initialAssetCount, finalAssetCount);
        Assert.Equal(initialWorkOrderCount, finalWorkOrderCount);
        Assert.Equal(initialOutboxCount, finalOutboxCount);
    }

    [Fact]
    public async Task WhenExceptionInPipeline_ShouldRollback_AllAggregateChanges()
    {
        // Arrange
        var assetId = await CreateAssetAsync("TEST-ROLLBACK-003", "Test Machine");
        var technicianId = await CreateTechnicianAsync("Test Tech", "Senior", 3);

        // Create work order successfully first
        var createCommand = new CreateWorkOrderCommand(assetId, "Test Work Order", "B1", "F1", "R1");
        var workOrderId = await Mediator.Send(createCommand);
        Assert.True(workOrderId.IsSuccess);

        // Verify initial state
        var asset = await WriteDbContext.Assets.FindAsync(assetId);
        Assert.Equal(AssetStatus.UnderMaintenance, asset.Status);

        // Act - Try to assign technician to non-existent work order (should cause exception)
        var nonExistentWorkOrderId = Guid.NewGuid();
        var assignCommand = new AssignTechnicianCommand(nonExistentWorkOrderId, technicianId);
        var result = await Mediator.Send(assignCommand);

        // Assert
        Assert.False(result.IsSuccess);

        // Verify technician wasn't assigned to anything
        var technician = await WriteDbContext.Technicians.FindAsync(technicianId);
        Assert.Empty(technician.Assignments);

        // Verify original work order state is unchanged
        var workOrder = await WriteDbContext.WorkOrders.FindAsync(workOrderId.Value);
        Assert.NotNull(workOrder);
        Assert.Equal(WorkOrderStatus.Open, workOrder.Status);
        Assert.Null(workOrder.TechnicianId);
    }

    [Fact]
    public async Task WhenConcurrencyConflictOccurs_ShouldRollback()
    {
        // Arrange
        var assetId = await CreateAssetAsync("TEST-ROLLBACK-004", "Test Machine");

        // Act - Try to create two work orders for the same asset simultaneously
        // This should cause a concurrency conflict in the domain event handler
        var command1 = new CreateWorkOrderCommand(assetId, "Work Order 1", "B1", "F1", "R1");
        var command2 = new CreateWorkOrderCommand(assetId, "Work Order 2", "B1", "F1", "R2");

        var task1 = ExecuteInIsolatedScopeAsync(async mediator =>
        {
            return await mediator.Send(command1);
        });

        var task2 = ExecuteInIsolatedScopeAsync(async mediator =>
        {
            return await mediator.Send(command2);
        });

        var act = () => Task.WhenAll(task1, task2);

        // Assert - One should succeed, one should fail
        await Assert.ThrowsAsync<DbUpdateConcurrencyException>(act);

        var workOrders = await WriteDbContext.WorkOrders
            .Where(wo => wo.AssetId == assetId)
            .ToListAsync();

        Assert.Single(workOrders);

        WriteDbContext.ChangeTracker.Clear();
        var asset = await WriteDbContext.Assets.FindAsync(assetId);
        Assert.Equal(AssetStatus.UnderMaintenance, asset.Status);
    }

    [Fact]
    public async Task WhenWorkOrderCompletionFails_ShouldRollback_AllChanges()
    {
        // Arrange
        var assetId = await CreateAssetAsync("TEST-ROLLBACK-005", "Test Machine");
        var technicianId = await CreateTechnicianAsync("Test Tech", "Senior", 3);

        // Create and assign work order
        var createCommand = new CreateWorkOrderCommand(assetId, "Test Work Order", "B1", "F1", "R1");
        var workOrderId = await Mediator.Send(createCommand);

        var assignCommand = new AssignTechnicianCommand(workOrderId.Value, technicianId);
        await Mediator.Send(assignCommand);

        // Act 
        var completeCommand = new CompleteWorkOrderCommand(workOrderId.Value);
        var act = () => Mediator.Send(completeCommand);

        // Assert
        await Assert.ThrowsAsync<DomainException>(act);

        // Verify work order is still in progress
        var workOrder = await WriteDbContext.WorkOrders.FindAsync(workOrderId.Value);
        Assert.Equal(WorkOrderStatus.Assigned, workOrder.Status);

        // Verify asset is still under maintenance
        var asset = await WriteDbContext.Assets.FindAsync(assetId);
        Assert.Equal(AssetStatus.UnderMaintenance, asset.Status);

        // Verify technician assignment is still active
        var technician = await WriteDbContext.Technicians.FindAsync(technicianId);
        var assignment = technician.Assignments.FirstOrDefault(a => a.WorkOrderId == workOrderId.Value);
        Assert.NotNull(assignment);
        Assert.False(assignment.IsCompleted);
    }

    [Fact]
    public async Task WhenAssetCreationFails_ShouldNotCreate_AnyRelatedEntities()
    {
        // Arrange
        var initialAssetCount = await WriteDbContext.Assets.CountAsync();
        var initialOutboxCount = await OutboxDbContext.OutboxMessages.CountAsync();

        var command = new CreateAssetCommand("Test Asset", "Equipment", null!, "Main Site", "Production", "Zone A");

        var act = () => Mediator.Send(command);

        // Assert
        await Assert.ThrowsAsync<DomainException>(act);

        // Verify no asset was created
        var finalAssetCount = await WriteDbContext.Assets.CountAsync();
        Assert.Equal(initialAssetCount, finalAssetCount);

        // Verify no outbox events were written
        var finalOutboxCount = await OutboxDbContext.OutboxMessages.CountAsync();
        Assert.Equal(initialOutboxCount, finalOutboxCount);
    }

    [Fact]
    public async Task WhenTechnicianCreationFails_ShouldNotCreate_AnyRelatedEntities()
    {
        // Arrange
        var initialTechnicianCount = await WriteDbContext.Technicians.CountAsync();
        var initialOutboxCount = await OutboxDbContext.OutboxMessages.CountAsync();

        var command = new CreateTechnicianCommand("", "", 3);

        var act = () => Mediator.Send(command);

        // Assert
        await Assert.ThrowsAsync<DomainException>(act);

        // Verify no technician was created
        var finalTechnicianCount = await WriteDbContext.Technicians.CountAsync();
        Assert.Equal(initialTechnicianCount, finalTechnicianCount);

        // Verify no outbox events were written
        var finalOutboxCount = await OutboxDbContext.OutboxMessages.CountAsync();
        Assert.Equal(initialOutboxCount, finalOutboxCount);
    }
}
