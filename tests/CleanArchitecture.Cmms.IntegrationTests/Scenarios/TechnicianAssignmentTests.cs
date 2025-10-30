using CleanArchitecture.Cmms.Application.Technicians.Commands.SetUnavailable;
using CleanArchitecture.Cmms.Application.Technicians.Queries.GetAvailableTechnicians;
using CleanArchitecture.Cmms.Application.Technicians.Queries.GetTechnicianAssignments;
using CleanArchitecture.Cmms.Application.WorkOrders.Commands.AssignTechnician;
using CleanArchitecture.Cmms.Application.WorkOrders.Commands.CompleteWorkOrder;
using CleanArchitecture.Cmms.Application.WorkOrders.Commands.CreateWorkOrder;
using CleanArchitecture.Cmms.Application.WorkOrders.Commands.StartWorkOrder;
using CleanArchitecture.Cmms.Domain.WorkOrders.Enums;
using CleanArchitecture.Cmms.Infrastructure.Persistence.EfCore;
using CleanArchitecture.Cmms.IntegrationTests.Infrastructure;
using CleanArchitecture.Core.Application.Abstractions.Query;
using CleanArchitecture.Core.Domain.Abstractions;
using Microsoft.EntityFrameworkCore;

namespace CleanArchitecture.Cmms.IntegrationTests.Scenarios;

/// <summary>
/// Integration tests for technician assignment scenarios
/// Tests complex assignment workflows and business rules
/// </summary>
public class TechnicianAssignmentTests : IntegrationTestBase
{
    public TechnicianAssignmentTests(CmmsWebApplicationFactory factory) : base(factory)
    {
    }

    [Fact]
    public async Task AssignTechnicianToWorkOrder_ShouldUpdateBothAggregates()
    {
        // Arrange
        var assetId = await CreateAssetAsync("ASSIGNMENT-TEST-001", "Assignment Test Machine");
        var technicianId = await CreateTechnicianAsync("Assignment Tech", "Senior", 3);

        var workOrderId = await Mediator.Send(new CreateWorkOrderCommand(assetId, "Assignment Test", "B1", "F1", "R1"));
        Assert.True(workOrderId.IsSuccess);

        // Act
        var assignCommand = new AssignTechnicianCommand(workOrderId.Value, technicianId);
        var result = await Mediator.Send(assignCommand);

        // Assert
        Assert.True(result.IsSuccess);

        // Verify work order was updated
        var workOrder = await WriteDbContext.WorkOrders.FindAsync(workOrderId.Value);
        Assert.Equal(WorkOrderStatus.Assigned, workOrder.Status);
        Assert.Equal(technicianId, workOrder.TechnicianId);

        // Verify technician was updated
        var technician = await WriteDbContext.Technicians.FindAsync(technicianId);
        Assert.Single(technician.Assignments.Where(a => a.WorkOrderId == workOrderId.Value));

        var assignment = technician.Assignments.First(a => a.WorkOrderId == workOrderId.Value);
        Assert.False(assignment.IsCompleted);
        Assert.Equal(workOrderId.Value, assignment.WorkOrderId);
    }

    [Fact]
    public async Task AssignUnavailableTechnician_ShouldFail()
    {
        // Arrange
        var assetId = await CreateAssetAsync("UNAVAILABLE-TEST-001", "Unavailable Test Machine");
        var technicianId = await CreateTechnicianAsync("Unavailable Tech", "Senior", 3);

        // Set technician as unavailable
        await Mediator.Send(new SetUnavailableCommand(technicianId));

        var workOrderId = await Mediator.Send(new CreateWorkOrderCommand(assetId, "Unavailable Test", "B1", "F1", "R1"));
        Assert.True(workOrderId.IsSuccess);

        // Act
        var assignCommand = new AssignTechnicianCommand(workOrderId.Value, technicianId);
        var act = () => Mediator.Send(assignCommand);

        // Assert
        await Assert.ThrowsAsync<DomainException>(act);

        WriteDbContext.ChangeTracker.Clear();
        var workOrder = await WriteDbContext.WorkOrders.FindAsync(workOrderId.Value);
        Assert.Equal(WorkOrderStatus.Open, workOrder.Status);
        Assert.Null(workOrder.TechnicianId);

        var technician = await WriteDbContext.Technicians.FindAsync(technicianId);
        Assert.Empty(technician.Assignments);
    }

    [Fact]
    public async Task AssignTechnicianToCompletedWorkOrder_ShouldFail()
    {
        // Arrange
        var assetId = await CreateAssetAsync("COMPLETED-TEST-001", "Completed Test Machine");
        var technicianId = await CreateTechnicianAsync("Completed Tech", "Senior", 3);

        var workOrderId = await Mediator.Send(new CreateWorkOrderCommand(assetId, "Completed Test", "B1", "F1", "R1"));
        await Mediator.Send(new AssignTechnicianCommand(workOrderId.Value, technicianId));
        await Mediator.Send(new StartWorkOrderCommand(workOrderId.Value));
        await Mediator.Send(new CompleteWorkOrderCommand(workOrderId.Value));

        // Verify work order is completed
        var workOrder = await WriteDbContext.WorkOrders.FindAsync(workOrderId.Value);
        Assert.Equal(WorkOrderStatus.Completed, workOrder.Status);

        // Act
        var anotherTechnicianId = await CreateTechnicianAsync("Another Tech", "Senior", 3);
        var assignCommand = new AssignTechnicianCommand(workOrderId.Value, anotherTechnicianId);
        var act = () => Mediator.Send(assignCommand);

        // Assert
        await Assert.ThrowsAsync<DomainException>(act);

        // Verify work order remains completed
        WriteDbContext.ChangeTracker.Clear();
        var updatedWorkOrder = await WriteDbContext.WorkOrders.FindAsync(workOrderId.Value);
        Assert.Equal(WorkOrderStatus.Completed, updatedWorkOrder.Status);
        Assert.Equal(technicianId, updatedWorkOrder.TechnicianId); // Original technician
    }

    [Fact]
    public async Task CompleteWorkOrderAssignment_ShouldFreeUpTechnician()
    {
        // Arrange
        var technicianId = await CreateTechnicianAsync("Free Up Tech", "Senior", 3);
        var assetId = await CreateAssetAsync("FREE-UP-TEST-001", "Free Up Test Machine");

        // Create and assign work order
        var workOrderId = await Mediator.Send(new CreateWorkOrderCommand(assetId, "Free Up Test", "B1", "F1", "R1"));
        await Mediator.Send(new AssignTechnicianCommand(workOrderId.Value, technicianId));
        await Mediator.Send(new StartWorkOrderCommand(workOrderId.Value));

        // Verify technician has active assignment
        var technicianBefore = await WriteDbContext.Technicians.FindAsync(technicianId);
        Assert.Single(technicianBefore.Assignments.Where(a => !a.IsCompleted));

        // Act - Complete work order
        await Mediator.Send(new CompleteWorkOrderCommand(workOrderId.Value));

        // Assert
        var technicianAfter = await WriteDbContext.Technicians.FindAsync(technicianId);
        var assignment = technicianAfter.Assignments.FirstOrDefault(a => a.WorkOrderId == workOrderId.Value);

        Assert.NotNull(assignment);
        Assert.True(assignment.IsCompleted);
        Assert.NotNull(assignment.CompletedOn);

        // Verify work order is completed
        var workOrder = await WriteDbContext.WorkOrders.FindAsync(workOrderId.Value);
        Assert.Equal(WorkOrderStatus.Completed, workOrder.Status);
    }

    [Fact]
    public async Task GetAvailableTechnicians_ShouldReturnOnlyAvailable()
    {
        // Arrange
        var availableTech1Id = await CreateTechnicianAsync("Available Tech 1", "Senior", 3);
        var availableTech2Id = await CreateTechnicianAsync("Available Tech 2", "Expert", 5);
        var unavailableTechId = await CreateTechnicianAsync("Unavailable Tech", "Junior", 1);

        // Set one technician as unavailable
        await Mediator.Send(new SetUnavailableCommand(unavailableTechId));

        // Act
        var query = new GetAvailableTechniciansQuery(new PaginationParam(1, 10));
        var result = await Mediator.Send(query);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);
        Assert.Equal(2, result.Value.Items.Count);
        Assert.Equal(2, result.Value.TotalCount);

        var technicianNames = result.Value.Items.Select(t => t.Name).ToList();
        Assert.Contains("Available Tech 1", technicianNames);
        Assert.Contains("Available Tech 2", technicianNames);
        Assert.DoesNotContain("Unavailable Tech", technicianNames);

        // Verify all returned technicians are available
        Assert.All(result.Value.Items, t => Assert.Equal("Available", t.Status));
    }

    [Fact]
    public async Task GetTechnicianAssignments_ShouldReturnCorrectAssignments()
    {
        // Arrange
        var technicianId = await CreateTechnicianAsync("Assignment Query Tech", "Senior", 3);
        var asset1Id = await CreateAssetAsync("ASSIGN-QUERY-001", "Machine 1");
        var asset2Id = await CreateAssetAsync("ASSIGN-QUERY-002", "Machine 2");

        // Create and assign work orders
        var workOrder1Id = await Mediator.Send(new CreateWorkOrderCommand(asset1Id, "Assignment Query 1", "B1", "F1", "R1"));
        var workOrder2Id = await Mediator.Send(new CreateWorkOrderCommand(asset2Id, "Assignment Query 2", "B1", "F1", "R2"));

        await Mediator.Send(new AssignTechnicianCommand(workOrder1Id.Value, technicianId));
        await Mediator.Send(new AssignTechnicianCommand(workOrder2Id.Value, technicianId));

        // Complete one work order
        await Mediator.Send(new StartWorkOrderCommand(workOrder1Id.Value));
        await Mediator.Send(new CompleteWorkOrderCommand(workOrder1Id.Value));

        // Act
        var query = new GetTechnicianAssignmentsQuery(technicianId, new PaginationParam(1, 10));
        var result = await Mediator.Send(query);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);
        Assert.Equal(2, result.Value.Items.Count);

        // Verify assignments
        var assignment1 = result.Value.Items.FirstOrDefault(a => a.WorkOrderId == workOrder1Id.Value);
        var assignment2 = result.Value.Items.FirstOrDefault(a => a.WorkOrderId == workOrder2Id.Value);

        Assert.NotNull(assignment1);
        Assert.NotNull(assignment2);
        Assert.True(assignment1.IsCompleted);
        Assert.False(assignment2.IsCompleted);
    }

    [Fact]
    public async Task TechnicianAssignment_ShouldTriggerDomainEvents()
    {
        // Arrange
        var assetId = await CreateAssetAsync("EVENT-TEST-001", "Event Test Machine");
        var technicianId = await CreateTechnicianAsync("Event Tech", "Senior", 3);

        var workOrderId = await Mediator.Send(new CreateWorkOrderCommand(assetId, "Event Test", "B1", "F1", "R1"));
        Assert.True(workOrderId.IsSuccess);

        // Act
        var assignCommand = new AssignTechnicianCommand(workOrderId.Value, technicianId);
        var result = await Mediator.Send(assignCommand);

        // Assert
        Assert.True(result.IsSuccess);

        // Verify integration events were written
        var outboxEvents = await OutboxDbContext.OutboxMessages
            .Where(e => e.EventType.Contains("TechnicianAssignedEvent"))
            .ToListAsync();

        Assert.Single(outboxEvents);
        var outboxEvent = outboxEvents[0];
        Assert.Contains(technicianId.ToString(), outboxEvent.Payload);
        Assert.Contains(workOrderId.Value.ToString(), outboxEvent.Payload);
    }

    [Fact]
    public async Task ReassignTechnician_ShouldUpdateBothAssignments()
    {
        // Arrange
        var assetId = await CreateAssetAsync("REASSIGN-TEST-001", "Reassign Test Machine");
        var technician1Id = await CreateTechnicianAsync("Tech 1", "Senior", 3);
        var technician2Id = await CreateTechnicianAsync("Tech 2", "Expert", 5);

        var workOrderId = await Mediator.Send(new CreateWorkOrderCommand(assetId, "Reassign Test", "B1", "F1", "R1"));
        Assert.True(workOrderId.IsSuccess);

        // Assign to first technician
        await Mediator.Send(new AssignTechnicianCommand(workOrderId.Value, technician1Id));

        // Act - Reassign to second technician
        var reassignCommand = new AssignTechnicianCommand(workOrderId.Value, technician2Id);
        var result = await Mediator.Send(reassignCommand);

        // Assert
        Assert.True(result.IsSuccess);

        WriteDbContext.ChangeTracker.Clear();

        var workOrder = await WriteDbContext.WorkOrders.FindAsync(workOrderId.Value);
        Assert.Equal(technician2Id, workOrder.TechnicianId);

        var technician1 = await WriteDbContext.Technicians.FindAsync(technician1Id);
        Assert.Empty(technician1.Assignments.Where(a => a.WorkOrderId == workOrderId.Value));

        var technician2 = await WriteDbContext.Technicians.FindAsync(technician2Id);
        Assert.Single(technician2.Assignments.Where(a => a.WorkOrderId == workOrderId.Value));
    }
}
