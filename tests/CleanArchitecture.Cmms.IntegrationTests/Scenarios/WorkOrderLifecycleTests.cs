using CleanArchitecture.Cmms.Application.Technicians.Commands.AddCertification;
using CleanArchitecture.Cmms.Application.Technicians.Commands.SetAvailable;
using CleanArchitecture.Cmms.Application.Technicians.Commands.SetUnavailable;
using CleanArchitecture.Cmms.Application.WorkOrders.Commands.AddStep;
using CleanArchitecture.Cmms.Application.WorkOrders.Commands.AssignTechnician;
using CleanArchitecture.Cmms.Application.WorkOrders.Commands.CompleteStep;
using CleanArchitecture.Cmms.Application.WorkOrders.Commands.CompleteWorkOrder;
using CleanArchitecture.Cmms.Application.WorkOrders.Commands.CreateWorkOrder;
using CleanArchitecture.Cmms.Application.WorkOrders.Commands.StartWorkOrder;
using CleanArchitecture.Cmms.Domain.Assets.Enums;
using CleanArchitecture.Cmms.Domain.Technicians.Enums;
using CleanArchitecture.Cmms.Domain.WorkOrders.Enums;
using CleanArchitecture.Cmms.Infrastructure.Persistence.EfCore;
using CleanArchitecture.Cmms.IntegrationTests.Infrastructure;
using CleanArchitecture.Core.Domain.Abstractions;
using Microsoft.EntityFrameworkCore;

namespace CleanArchitecture.Cmms.IntegrationTests.Scenarios;

/// <summary>
/// End-to-end integration tests for complete workflows
/// Tests real business scenarios from start to finish
/// </summary>
public class WorkOrderLifecycleTests : IntegrationTestBase
{
    public WorkOrderLifecycleTests(CmmsWebApplicationFactory factory) : base(factory)
    {
    }

    [Fact]
    public async Task CompleteWorkOrderLifecycle_ShouldUpdateAllAggregates()
    {
        // Arrange
        var assetId = await CreateAssetAsync("PROD-MACHINE-001", "Production Machine");
        var technicianId = await CreateTechnicianAsync("John Smith", "Senior", 3);

        // Act - Complete work order lifecycle
        // Create work order
        var createCommand = new CreateWorkOrderCommand(assetId, "Routine Maintenance", "Plant A", "Floor 2", "Room 205");
        var workOrderId = await Mediator.Send(createCommand);
        Assert.True(workOrderId.IsSuccess);

        // Assign technician
        var assignCommand = new AssignTechnicianCommand(workOrderId.Value, technicianId);
        var assignResult = await Mediator.Send(assignCommand);
        Assert.True(assignResult.IsSuccess);

        // Start work order
        var startCommand = new StartWorkOrderCommand(workOrderId.Value);
        var startResult = await Mediator.Send(startCommand);
        Assert.True(startResult.IsSuccess);

        // Add work steps
        var addStepCommand1 = new AddStepCommand(workOrderId.Value, "Inspect machine components");

        var step = await Mediator.Send(addStepCommand1);

        // Complete the step
        var completeStep = new CompleteStepCommand(workOrderId.Value, step.Value);

        await Mediator.Send(completeStep);

        // Complete work order
        var completeCommand = new CompleteWorkOrderCommand(workOrderId.Value);
        var completeResult = await Mediator.Send(completeCommand);
        Assert.True(completeResult.IsSuccess);

        // Assert - Verify all aggregates are in correct final state
        WriteDbContext.ChangeTracker.Clear();
        var workOrder = await WriteDbContext.WorkOrders.FindAsync(workOrderId.Value);
        var asset = await WriteDbContext.Assets.FindAsync(assetId);
        var technician = await WriteDbContext.Technicians.FindAsync(technicianId);

        // Work Order should be completed
        Assert.Equal(WorkOrderStatus.Completed, workOrder.Status);
        Assert.Equal(technicianId, workOrder.TechnicianId);
        Assert.Single(workOrder.Steps);
        Assert.All(workOrder.Steps, step => Assert.True(step.Completed));

        // Asset should be back to active
        Assert.Equal(AssetStatus.Active, asset.Status);
        Assert.Single(asset.MaintenanceRecords);

        var assignment = technician.Assignments.FirstOrDefault(a => a.WorkOrderId == workOrderId.Value);
        Assert.NotNull(assignment);
        Assert.True(assignment.IsCompleted);
        Assert.NotNull(assignment.CompletedOn);

        var outboxEvents = await OutboxDbContext.OutboxMessages
            .OrderBy(e => e.CreatedAt)
            .ToListAsync();

        Assert.True(outboxEvents.Count >= 4);
    }

    [Fact]
    public async Task TechnicianAssignmentWorkflow_ShouldHandleMultipleAssignments()
    {
        // Arrange
        var technicianId = await CreateTechnicianAsync("Multi-Task Tech", "Expert", 5);
        var asset1Id = await CreateAssetAsync("MACHINE-001", "Machine A");
        var asset2Id = await CreateAssetAsync("MACHINE-002", "Machine B");

        // Act
        var workOrder1Id = await Mediator.Send(new CreateWorkOrderCommand(asset1Id, "Repair Machine A", "B1", "F1", "R1"));
        var workOrder2Id = await Mediator.Send(new CreateWorkOrderCommand(asset2Id, "Repair Machine B", "B1", "F1", "R2"));

        await Mediator.Send(new AssignTechnicianCommand(workOrder1Id.Value, technicianId));
        await Mediator.Send(new AssignTechnicianCommand(workOrder2Id.Value, technicianId));

        // Assert
        var technician = await WriteDbContext.Technicians.FindAsync(technicianId);
        Assert.Equal(2, technician.Assignments.Count(a => !a.IsCompleted));

        var workOrder1 = await WriteDbContext.WorkOrders.FindAsync(workOrder1Id.Value);
        var workOrder2 = await WriteDbContext.WorkOrders.FindAsync(workOrder2Id.Value);

        Assert.Equal(WorkOrderStatus.Assigned, workOrder1.Status);
        Assert.Equal(WorkOrderStatus.Assigned, workOrder2.Status);
        Assert.Equal(technicianId, workOrder1.TechnicianId);
        Assert.Equal(technicianId, workOrder2.TechnicianId);

        var asset1 = await WriteDbContext.Assets.FindAsync(asset1Id);
        var asset2 = await WriteDbContext.Assets.FindAsync(asset2Id);

        Assert.Equal(AssetStatus.UnderMaintenance, asset1.Status);
        Assert.Equal(AssetStatus.UnderMaintenance, asset2.Status);
    }

    [Fact]
    public async Task AssetMaintenanceWorkflow_ShouldTrackCompleteHistory()
    {
        // Arrange
        var assetId = await CreateAssetAsync("HISTORY-MACHINE-001", "History Tracking Machine");
        var technicianId = await CreateTechnicianAsync("History Tech", "Senior", 3);

        // Act - Complete multiple maintenance cycles
        // First maintenance cycle
        var workOrder1Id = await Mediator.Send(new CreateWorkOrderCommand(assetId, "First Maintenance", "B1", "F1", "R1"));
        await Mediator.Send(new AssignTechnicianCommand(workOrder1Id.Value, technicianId));
        await Mediator.Send(new StartWorkOrderCommand(workOrder1Id.Value));
        await Mediator.Send(new CompleteWorkOrderCommand(workOrder1Id.Value));

        // Second maintenance cycle
        var workOrder2Id = await Mediator.Send(new CreateWorkOrderCommand(assetId, "Second Maintenance", "B1", "F1", "R1"));
        await Mediator.Send(new AssignTechnicianCommand(workOrder2Id.Value, technicianId));
        await Mediator.Send(new StartWorkOrderCommand(workOrder2Id.Value));
        await Mediator.Send(new CompleteWorkOrderCommand(workOrder2Id.Value));

        // Assert
        var asset = await WriteDbContext.Assets.FindAsync(assetId);
        Assert.Equal(AssetStatus.Active, asset.Status);
        Assert.Equal(2, asset.MaintenanceRecords.Count);

        var maintenanceRecords = asset.MaintenanceRecords.OrderBy(r => r.StartedOn).ToList();
        Assert.Contains("First Maintenance", maintenanceRecords[0].Description);
        Assert.Contains("Second Maintenance", maintenanceRecords[1].Description);

        var workOrder1 = await WriteDbContext.WorkOrders.FindAsync(workOrder1Id.Value);
        var workOrder2 = await WriteDbContext.WorkOrders.FindAsync(workOrder2Id.Value);

        Assert.Equal(WorkOrderStatus.Completed, workOrder1.Status);
        Assert.Equal(WorkOrderStatus.Completed, workOrder2.Status);
    }

    [Fact]
    public async Task TechnicianCertificationWorkflow_ShouldTrackQualifications()
    {
        // Arrange
        var technicianId = await CreateTechnicianAsync("Certified Tech", "Senior", 3);

        // Act 
        await Mediator.Send(new AddCertificationCommand(technicianId, "WW-SOL-001", DateTime.UtcNow.AddDays(-30), DateTime.UtcNow.AddDays(335)));
        await Mediator.Send(new AddCertificationCommand(technicianId, "AA-ADV-002", DateTime.UtcNow.AddDays(-60), DateTime.UtcNow.AddDays(305)));
        await Mediator.Send(new AddCertificationCommand(technicianId, "AA-PRO-003", DateTime.UtcNow.AddDays(-90), null)); // No expiration

        // Assert
        var technician = await WriteDbContext.Technicians.FindAsync(technicianId);
        Assert.Equal(3, technician.Certifications.Count);

        var certifications = technician.Certifications.ToList();
        Assert.Contains(certifications, c => c.Code == "WW-SOL-001");
        Assert.Contains(certifications, c => c.Code == "AA-ADV-002");
        Assert.Contains(certifications, c => c.Code == "AA-PRO-003");

        var awsCert = certifications.First(c => c.Code == "WW-SOL-001");
        Assert.Equal("WW-SOL-001", awsCert.Code);
        Assert.NotNull(awsCert.ExpiresOn);

        var gcpCert = certifications.First(c => c.Code == "AA-PRO-003");
        Assert.Equal("AA-PRO-003", gcpCert.Code);
        Assert.Null(gcpCert.ExpiresOn);
    }

    [Fact]
    public async Task TechnicianAvailabilityWorkflow_ShouldControlAssignments()
    {
        // Arrange
        var technicianId = await CreateTechnicianAsync("Availability Tech", "Senior", 3);
        var assetId = await CreateAssetAsync("AVAILABILITY-MACHINE-001", "Availability Test Machine");

        // Act
        // 1. Create work order while technician is available
        var workOrderId = await Mediator.Send(new CreateWorkOrderCommand(assetId, "Availability Test", "B1", "F1", "R1"));
        var assignResult = await Mediator.Send(new AssignTechnicianCommand(workOrderId.Value, technicianId));
        Assert.True(assignResult.IsSuccess);

        // 2. Set technician as unavailable
        await Mediator.Send(new SetUnavailableCommand(technicianId));

        // 3. Try to assign another work order (should fail)
        var asset2Id = await CreateAssetAsync("AVAILABILITY-MACHINE-002", "Second Machine");
        var workOrder2Id = await Mediator.Send(new CreateWorkOrderCommand(asset2Id, "Second Work Order", "B1", "F1", "R2"));
        var assignResult2Act = () => Mediator.Send(new AssignTechnicianCommand(workOrder2Id.Value, technicianId));
        await Assert.ThrowsAsync<DomainException>(assignResult2Act);

        // 4. Set technician as available again
        await Mediator.Send(new SetAvailableCommand(technicianId));

        // 5. Now assignment should succeed
        var assignResult3 = await Mediator.Send(new AssignTechnicianCommand(workOrder2Id.Value, technicianId));
        Assert.True(assignResult3.IsSuccess);

        // Assert
        var technician = await WriteDbContext.Technicians.FindAsync(technicianId);
        Assert.Equal(TechnicianStatus.Available, technician.Status);
        Assert.Equal(2, technician.Assignments.Count(a => !a.IsCompleted));

        var workOrder1 = await WriteDbContext.WorkOrders.FindAsync(workOrderId.Value);
        var workOrder2 = await WriteDbContext.WorkOrders.FindAsync(workOrder2Id.Value);

        Assert.Equal(WorkOrderStatus.Assigned, workOrder1.Status);
        Assert.Equal(WorkOrderStatus.Assigned, workOrder2.Status);
    }

    [Fact]
    public async Task ComplexWorkOrderWorkflow_ShouldHandleRealWorldScenario()
    {
        // Arrange
        var productionMachineId = await CreateAssetAsync("PROD-001", "Production Line Machine");
        var seniorTechnicianId = await CreateTechnicianAsync("Mike", "Senior", 4);
        var juniorTechnicianId = await CreateTechnicianAsync("Sarah", "Junior", 2);

        await Mediator.Send(new AddCertificationCommand(seniorTechnicianId, "MACHINE-SAFETY-001", DateTime.UtcNow.AddDays(-30), DateTime.UtcNow.AddDays(335)));

        // Act

        // 1. Create emergency work order
        var emergencyWorkOrderId = await Mediator.Send(new CreateWorkOrderCommand(productionMachineId, "Emergency Repair - Machine Down", "Production Floor", "Level 1", "Station A"));
        Assert.True(emergencyWorkOrderId.IsSuccess);

        // 2. Assign senior technician
        await Mediator.Send(new AssignTechnicianCommand(emergencyWorkOrderId.Value, seniorTechnicianId));
        await Mediator.Send(new StartWorkOrderCommand(emergencyWorkOrderId.Value));

        // 3. Add detailed work steps
        var step = await Mediator.Send(new AddStepCommand(emergencyWorkOrderId.Value, "Safety check and lockout"));

        //4. Complete Step
        var completeStep = new CompleteStepCommand(emergencyWorkOrderId.Value, step.Value);
        await Mediator.Send(completeStep);

        // 5. Complete work order
        await Mediator.Send(new CompleteWorkOrderCommand(emergencyWorkOrderId.Value));

        // 6. Create follow-up maintenance work order
        var followUpWorkOrderId = await Mediator.Send(new CreateWorkOrderCommand(productionMachineId, "Follow-up Inspection", "Production Floor", "Level 1", "Station A"));
        await Mediator.Send(new AssignTechnicianCommand(followUpWorkOrderId.Value, juniorTechnicianId));
        await Mediator.Send(new StartWorkOrderCommand(followUpWorkOrderId.Value));
        var visualStep = await Mediator.Send(new AddStepCommand(followUpWorkOrderId.Value, "Visual inspection"));
        var performanceStep = await Mediator.Send(new AddStepCommand(followUpWorkOrderId.Value, "Performance test"));
        await Mediator.Send(new CompleteStepCommand(followUpWorkOrderId.Value, visualStep.Value));
        await Mediator.Send(new CompleteStepCommand(followUpWorkOrderId.Value, performanceStep.Value));
        await Mediator.Send(new CompleteWorkOrderCommand(followUpWorkOrderId.Value));

        // Assert 
        WriteDbContext.ChangeTracker.Clear();
        var productionMachine = await WriteDbContext.Assets.FindAsync(productionMachineId);
        var seniorTech = await WriteDbContext.Technicians.FindAsync(seniorTechnicianId);
        var juniorTech = await WriteDbContext.Technicians.FindAsync(juniorTechnicianId);

        // Asset should be active after both work orders completed
        Assert.Equal(AssetStatus.Active, productionMachine.Status);
        Assert.Equal(2, productionMachine.MaintenanceRecords.Count);

        // Senior technician should have completed emergency work order
        var seniorAssignment = seniorTech.Assignments.FirstOrDefault(a => a.WorkOrderId == emergencyWorkOrderId.Value);
        Assert.NotNull(seniorAssignment);
        Assert.True(seniorAssignment.IsCompleted);

        // Junior technician should have completed follow-up work order
        var juniorAssignment = juniorTech.Assignments.FirstOrDefault(a => a.WorkOrderId == followUpWorkOrderId.Value);
        Assert.NotNull(juniorAssignment);
        Assert.True(juniorAssignment.IsCompleted);

        // Verify work orders are completed
        var emergencyWorkOrder = await WriteDbContext.WorkOrders.FindAsync(emergencyWorkOrderId.Value);
        var followUpWorkOrder = await WriteDbContext.WorkOrders.FindAsync(followUpWorkOrderId.Value);

        Assert.Equal(WorkOrderStatus.Completed, emergencyWorkOrder.Status);
        Assert.Equal(WorkOrderStatus.Completed, followUpWorkOrder.Status);
        Assert.Equal(2, followUpWorkOrder.Steps.Count);

        var outboxEvents = await OutboxDbContext.OutboxMessages
            .OrderBy(e => e.CreatedAt)
            .ToListAsync();

        Assert.True(outboxEvents.Count >= 6);
    }

}
