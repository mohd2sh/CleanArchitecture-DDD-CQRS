using CleanArchitecture.Cmms.Application.WorkOrders.Commands.AssignTechnician;
using CleanArchitecture.Cmms.Application.WorkOrders.Commands.CompleteWorkOrder;
using CleanArchitecture.Cmms.Application.WorkOrders.Commands.CreateWorkOrder;
using CleanArchitecture.Cmms.Application.WorkOrders.Commands.StartWorkOrder;
using CleanArchitecture.Cmms.Domain.Assets.Enums;
using CleanArchitecture.Cmms.Domain.WorkOrders.Enums;
using CleanArchitecture.Cmms.IntegrationTests.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace CleanArchitecture.Cmms.IntegrationTests.Events;

public class DomainEventTests : IntegrationTestBase
{
    public DomainEventTests(CmmsWebApplicationFactory factory) : base(factory)
    {
    }

    [Fact]
    public async Task WorkOrderCreated_ShouldTrigger_AssetUnderMaintenanceEvent()
    {
        // Arrange
        var assetId = await CreateAssetAsync("TEST-001", "Test Machine");

        // Act
        var command = new CreateWorkOrderCommand(
            AssetId: assetId,
            Title: "Fix Machine",
            Building: "Building A",
            Floor: "Floor 1",
            Room: "Room 101");

        var result = await Mediator.Send(command);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.NotEqual(Guid.Empty, result.Value);

        // Verify asset status changed to UnderMaintenance (via domain event handler)
        var asset = await WriteDbContext.Assets.FindAsync(assetId);
        Assert.NotNull(asset);
        Assert.Equal(AssetStatus.UnderMaintenance, asset.Status);

        // Verify work order was created
        var workOrder = await WriteDbContext.WorkOrders.FindAsync(result.Value);
        Assert.NotNull(workOrder);
        Assert.Equal("Fix Machine", workOrder.Title);
        Assert.Equal(WorkOrderStatus.Open, workOrder.Status);
    }

    [Fact]
    public async Task WorkOrderCompleted_ShouldTrigger_AssetAvailableAndTechnicianFreeEvents()
    {
        // Arrange
        var assetId = await CreateAssetAsync("TEST-002", "Test Machine");
        var technicianId = await CreateTechnicianAsync("Jane Doe", "Senior", 3);

        // Create and assign work order
        var createCommand = new CreateWorkOrderCommand(assetId, "Fix Machine", "B1", "F1", "R1");
        var workOrderId = await Mediator.Send(createCommand);

        var assignCommand = new AssignTechnicianCommand(workOrderId.Value, technicianId);
        await Mediator.Send(assignCommand);

        // Start work order
        var startCommand = new StartWorkOrderCommand(workOrderId.Value);
        await Mediator.Send(startCommand);

        // Act - Complete work order
        var completeCommand = new CompleteWorkOrderCommand(workOrderId.Value);
        var result = await Mediator.Send(completeCommand);

        // Assert
        Assert.True(result.IsSuccess);

        // Verify asset status changed back to Active (via domain event handler)
        var asset = await WriteDbContext.Assets.FindAsync(assetId);
        Assert.NotNull(asset);
        Assert.Equal(AssetStatus.Active, asset.Status);

        // Verify work order is completed
        var workOrder = await WriteDbContext.WorkOrders.FindAsync(workOrderId.Value);
        Assert.NotNull(workOrder);
        Assert.Equal(WorkOrderStatus.Completed, workOrder.Status);

        // Verify technician assignment is completed
        var technician = await WriteDbContext.Technicians.FindAsync(technicianId);
        Assert.NotNull(technician);
        var assignment = technician.Assignments.FirstOrDefault(a => a.WorkOrderId == workOrderId.Value);
        Assert.NotNull(assignment);
        Assert.True(assignment.IsCompleted);
    }

    [Fact]
    public async Task DomainEvents_ShouldExecute_InSameTransaction()
    {
        // Arrange
        var assetId = await CreateAssetAsync("TEST-003", "Test Machine");

        // Act
        var command = new CreateWorkOrderCommand(assetId, "Fix Machine", "B1", "F1", "R1");
        var result = await Mediator.Send(command);

        // Assert
        Assert.True(result.IsSuccess);

        // Verify both work order creation and asset status change happened atomically
        var workOrder = await WriteDbContext.WorkOrders.FindAsync(result.Value);
        var asset = await WriteDbContext.Assets.FindAsync(assetId);

        Assert.NotNull(workOrder);
        Assert.NotNull(asset);
        Assert.Equal(AssetStatus.UnderMaintenance, asset.Status);
        Assert.Equal(WorkOrderStatus.Open, workOrder.Status);

        // Verify RowVersion was updated (indicating transaction committed)
        Assert.NotNull(asset.RowVersion);
        Assert.NotNull(workOrder.RowVersion);
    }

    [Fact]
    public async Task MultipleEventHandlers_ShouldExecute_InCorrectOrder()
    {
        // Arrange
        var assetId = await CreateAssetAsync("TEST-004", "Test Machine");
        var technicianId = await CreateTechnicianAsync("John Smith", "Senior", 3);

        // Act - Create work order (triggers WorkOrderCreatedEvent)
        var createCommand = new CreateWorkOrderCommand(assetId, "Fix Machine", "B1", "F1", "R1");
        var workOrderId = await Mediator.Send(createCommand);

        // Assign technician (triggers TechnicianAssignedEvent)
        var assignCommand = new AssignTechnicianCommand(workOrderId.Value, technicianId);
        await Mediator.Send(assignCommand);

        // Assert - Verify correct sequence of events
        var workOrder = await WriteDbContext.WorkOrders.FindAsync(workOrderId.Value);
        var asset = await WriteDbContext.Assets.FindAsync(assetId);
        var technician = await WriteDbContext.Technicians.FindAsync(technicianId);

        Assert.NotNull(workOrder);
        Assert.NotNull(asset);
        Assert.NotNull(technician);

        // Work order should be assigned
        Assert.Equal(WorkOrderStatus.Assigned, workOrder.Status);
        Assert.Equal(technicianId, workOrder.TechnicianId);

        // Asset should be under maintenance
        Assert.Equal(AssetStatus.UnderMaintenance, asset.Status);

        // Technician should have assignment
        Assert.Single(technician.Assignments.Where(a => a.WorkOrderId == workOrderId.Value));
    }

    [Fact]
    public async Task IntegrationEvents_ShouldBeWritten_ToOutboxInTransaction()
    {
        // Arrange
        var assetId = await CreateAssetAsync("TEST-005", "Test Machine");

        // Act
        var command = new CreateWorkOrderCommand(assetId, "Fix Machine", "B1", "F1", "R1");
        var result = await Mediator.Send(command);

        // Assert
        Assert.True(result.IsSuccess);

        // Verify integration event written to outbox
        var outboxEvents = await OutboxDbContext.OutboxMessages
            .Where(e => e.EventType.Contains("WorkOrderCreatedEvent"))
            .ToListAsync();

        Assert.Single(outboxEvents);
        Assert.Contains("WorkOrderCreatedEvent", outboxEvents[0].EventType);
        Assert.NotNull(outboxEvents[0].Payload);
        Assert.Null(outboxEvents[0].ProcessedAt); // Not yet processed
        Assert.Equal(0, outboxEvents[0].RetryCount);
    }

    [Fact]
    public async Task CrossAggregateCoordination_ShouldWork_ViaDomainEvents()
    {
        // Arrange
        var assetId = await CreateAssetAsync("TEST-006", "Test Machine");
        var technicianId = await CreateTechnicianAsync("Alice Johnson", "Senior", 3);

        // Act - Complete workflow
        var createCommand = new CreateWorkOrderCommand(assetId, "Fix Machine", "B1", "F1", "R1");
        var workOrderId = await Mediator.Send(createCommand);

        var assignCommand = new AssignTechnicianCommand(workOrderId.Value, technicianId);
        await Mediator.Send(assignCommand);

        var startCommand = new StartWorkOrderCommand(workOrderId.Value);
        await Mediator.Send(startCommand);

        var completeCommand = new CompleteWorkOrderCommand(workOrderId.Value);
        await Mediator.Send(completeCommand);

        // Assert - Verify cross-aggregate coordination worked
        var workOrder = await WriteDbContext.WorkOrders.FindAsync(workOrderId.Value);
        var asset = await WriteDbContext.Assets.FindAsync(assetId);
        var technician = await WriteDbContext.Technicians.FindAsync(technicianId);

        // All aggregates should be in correct final state
        Assert.Equal(WorkOrderStatus.Completed, workOrder.Status);
        Assert.Equal(AssetStatus.Active, asset.Status);

        var assignment = technician.Assignments.FirstOrDefault(a => a.WorkOrderId == workOrderId.Value);
        Assert.NotNull(assignment);
        Assert.True(assignment.IsCompleted);

        // Verify integration events were written to outbox for each step
        var outboxEvents = await OutboxDbContext.OutboxMessages
            .OrderBy(e => e.CreatedAt)
            .ToListAsync();

        Assert.True(outboxEvents.Count >= 3); // At least WorkOrderCreated, TechnicianAssigned, WorkOrderCompleted
    }
}
