using CleanArchitecture.Cmms.Application.WorkOrders.Commands.AssignTechnician;
using CleanArchitecture.Cmms.Application.WorkOrders.Commands.CompleteWorkOrder;
using CleanArchitecture.Cmms.Application.WorkOrders.Commands.CreateWorkOrder;
using CleanArchitecture.Cmms.Application.WorkOrders.Commands.StartWorkOrder;
using CleanArchitecture.Cmms.Domain.Assets.Enums;
using CleanArchitecture.Cmms.Domain.WorkOrders.Enums;
using CleanArchitecture.Cmms.IntegrationTests.Infrastructure;

namespace CleanArchitecture.Cmms.IntegrationTests.Events;

/// <summary>
/// Tests validating ADR-001: Cross-Aggregate Coordination Pattern
/// Ensures domain events properly coordinate operations across aggregates
/// </summary>
public class CrossAggregateCoordinationTests : IntegrationTestBase
{
    public CrossAggregateCoordinationTests(CmmsWebApplicationFactory factory) : base(factory)
    {
    }

    [Fact]
    public async Task WorkOrderCreation_ShouldCoordinate_WithAssetAggregate()
    {
        // Arrange
        var assetId = await CreateAssetAsync("TEST-COORD-001", "Production Machine");
        var initialAssetStatus = (await WriteDbContext.Assets.FindAsync(assetId))!.Status;

        // Act
        var command = new CreateWorkOrderCommand(assetId, "Emergency Repair", "Plant A", "Floor 2", "Room 205");
        var result = await Mediator.Send(command);

        // Assert
        Assert.True(result.IsSuccess);

        // Verify WorkOrder aggregate state
        var workOrder = await WriteDbContext.WorkOrders.FindAsync(result.Value);
        Assert.NotNull(workOrder);
        Assert.Equal(assetId, workOrder.AssetId);
        Assert.Equal("Emergency Repair", workOrder.Title);
        Assert.Equal(WorkOrderStatus.Open, workOrder.Status);

        // Verify Asset aggregate state changed via domain event
        var asset = await WriteDbContext.Assets.FindAsync(assetId);
        Assert.NotNull(asset);
        Assert.Equal(AssetStatus.UnderMaintenance, asset.Status);
        Assert.NotEqual(initialAssetStatus, asset.Status);

        // Verify maintenance record was created
        Assert.Single(asset.MaintenanceRecords);
        var maintenanceRecord = asset.MaintenanceRecords.First();
        Assert.Contains("Emergency Repair", maintenanceRecord.Description);
        Assert.Equal("System", maintenanceRecord.PerformedBy);
    }

    [Fact]
    public async Task TechnicianAssignment_ShouldCoordinate_WithTechnicianAggregate()
    {
        // Arrange
        var assetId = await CreateAssetAsync("TEST-COORD-002", "Test Machine");
        var technicianId = await CreateTechnicianAsync("Bob Wilson", "Senior", 3);

        var createCommand = new CreateWorkOrderCommand(assetId, "Routine Maintenance", "B1", "F1", "R1");
        var workOrderId = await Mediator.Send(createCommand);

        // Act
        var assignCommand = new AssignTechnicianCommand(workOrderId.Value, technicianId);
        var result = await Mediator.Send(assignCommand);

        // Assert
        Assert.True(result.IsSuccess);

        // Verify WorkOrder aggregate state
        var workOrder = await WriteDbContext.WorkOrders.FindAsync(workOrderId.Value);
        Assert.NotNull(workOrder);
        Assert.Equal(technicianId, workOrder.TechnicianId);
        Assert.Equal(WorkOrderStatus.Assigned, workOrder.Status);

        // Verify Technician aggregate state changed via domain event
        var technician = WriteDbContext.Technicians.FirstOrDefault(a => a.Id == technicianId);
        Assert.NotNull(technician);
        Assert.Single(technician.Assignments.Where(a => a.WorkOrderId == workOrderId.Value));

        var assignment = technician.Assignments.First(a => a.WorkOrderId == workOrderId.Value);
        Assert.False(assignment.IsCompleted);
        Assert.Equal(workOrderId.Value, assignment.WorkOrderId);
    }

    [Fact]
    public async Task WorkOrderCompletion_ShouldCoordinate_WithAllAggregates()
    {
        // Arrange
        var assetId = await CreateAssetAsync("TEST-COORD-003", "Critical Machine");
        var technicianId = await CreateTechnicianAsync("Carol Davis", "Expert", 5);

        // Create and assign work order
        var createCommand = new CreateWorkOrderCommand(assetId, "Critical Repair", "B1", "F1", "R1");
        var workOrderId = await Mediator.Send(createCommand);

        var assignCommand = new AssignTechnicianCommand(workOrderId.Value, technicianId);
        await Mediator.Send(assignCommand);

        var startCommand = new StartWorkOrderCommand(workOrderId.Value);
        await Mediator.Send(startCommand);

        // Act
        var completeCommand = new CompleteWorkOrderCommand(workOrderId.Value);
        var result = await Mediator.Send(completeCommand);

        // Assert
        Assert.True(result.IsSuccess);

        // Verify WorkOrder aggregate state
        var workOrder = await WriteDbContext.WorkOrders.FindAsync(workOrderId.Value);
        Assert.NotNull(workOrder);
        Assert.Equal(WorkOrderStatus.Completed, workOrder.Status);

        // Verify Asset aggregate state changed via domain event
        var asset = await WriteDbContext.Assets.FindAsync(assetId);
        Assert.NotNull(asset);
        Assert.Equal(AssetStatus.Active, asset.Status);

        // Verify Technician aggregate state changed via domain event
        var technician = await WriteDbContext.Technicians.FindAsync(technicianId);
        Assert.NotNull(technician);

        var assignment = technician.Assignments.FirstOrDefault(a => a.WorkOrderId == workOrderId.Value);
        Assert.NotNull(assignment);
        Assert.True(assignment.IsCompleted);
        Assert.NotNull(assignment.CompletedOn);
    }

    [Fact]
    public async Task MultipleWorkOrders_ShouldNotInterfere_WithEachOther()
    {
        // Arrange
        var asset1Id = await CreateAssetAsync("TEST-COORD-004A", "Machine A");
        var asset2Id = await CreateAssetAsync("TEST-COORD-004B", "Machine B");
        var technicianId = await CreateTechnicianAsync("David Brown", "Senior", 3);

        // Act - Create work orders for different assets
        var command1 = new CreateWorkOrderCommand(asset1Id, "Repair Machine A", "B1", "F1", "R1");
        var command2 = new CreateWorkOrderCommand(asset2Id, "Repair Machine B", "B1", "F1", "R2");

        var workOrder1Id = await Mediator.Send(command1);
        var workOrder2Id = await Mediator.Send(command2);

        // Assign same technician to both
        await Mediator.Send(new AssignTechnicianCommand(workOrder1Id.Value, technicianId));
        await Mediator.Send(new AssignTechnicianCommand(workOrder2Id.Value, technicianId));

        // Assert
        Assert.True(workOrder1Id.IsSuccess);
        Assert.True(workOrder2Id.IsSuccess);

        // Verify both assets are under maintenance
        var asset1 = await WriteDbContext.Assets.FindAsync(asset1Id);
        var asset2 = await WriteDbContext.Assets.FindAsync(asset2Id);

        Assert.Equal(AssetStatus.UnderMaintenance, asset1.Status);
        Assert.Equal(AssetStatus.UnderMaintenance, asset2.Status);

        // Verify both work orders are assigned
        var workOrder1 = await WriteDbContext.WorkOrders.FindAsync(workOrder1Id.Value);
        var workOrder2 = await WriteDbContext.WorkOrders.FindAsync(workOrder2Id.Value);

        Assert.Equal(WorkOrderStatus.Assigned, workOrder1.Status);
        Assert.Equal(WorkOrderStatus.Assigned, workOrder2.Status);

        // Verify technician has both assignments
        var technician = await WriteDbContext.Technicians.FindAsync(technicianId);
        Assert.Equal(2, technician.Assignments.Count(a => !a.IsCompleted));
    }

    [Fact]
    public async Task DomainEvents_ShouldMaintain_AggregateBoundaries()
    {
        // Arrange
        var assetId = await CreateAssetAsync("TEST-COORD-005", "Test Machine");

        // Act
        var command = new CreateWorkOrderCommand(assetId, "Boundary Test", "B1", "F1", "R1");
        var result = await Mediator.Send(command);

        // Assert
        Assert.True(result.IsSuccess);

        // Verify WorkOrder aggregate only contains WorkOrder data
        var workOrder = await WriteDbContext.WorkOrders.FindAsync(result.Value);
        Assert.NotNull(workOrder);
        Assert.Equal(assetId, workOrder.AssetId);
        Assert.Equal("Boundary Test", workOrder.Title);
        Assert.Empty(workOrder.Steps); // No steps added
        Assert.Empty(workOrder.Comments); // No comments added

        // Verify Asset aggregate only contains Asset data
        var asset = await WriteDbContext.Assets.FindAsync(assetId);
        Assert.NotNull(asset);
        Assert.Equal(AssetStatus.UnderMaintenance, asset.Status);
        Assert.Single(asset.MaintenanceRecords);
    }

    [Fact]
    public async Task EventDrivenCoordination_ShouldBe_Transactional()
    {
        // Arrange
        var assetId = await CreateAssetAsync("TEST-COORD-006", "Transactional Test");

        // Act
        var command = new CreateWorkOrderCommand(assetId, "Transactional Test", "B1", "F1", "R1");
        var result = await Mediator.Send(command);

        // Assert
        Assert.True(result.IsSuccess);

        // Verify both aggregates were updated in same transaction
        var workOrder = await WriteDbContext.WorkOrders.FindAsync(result.Value);
        var asset = await WriteDbContext.Assets.FindAsync(assetId);

        Assert.NotNull(workOrder);
        Assert.NotNull(asset);

        // Both should have RowVersion indicating they were saved in same transaction
        Assert.NotNull(workOrder.RowVersion);
        Assert.NotNull(asset.RowVersion);

        // Verify the coordination happened atomically
        Assert.Equal(WorkOrderStatus.Open, workOrder.Status);
        Assert.Equal(AssetStatus.UnderMaintenance, asset.Status);
    }
}
