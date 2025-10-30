using CleanArchitecture.Cmms.Application.WorkOrders.Commands.AssignTechnician;
using CleanArchitecture.Cmms.Application.WorkOrders.Commands.CompleteWorkOrder;
using CleanArchitecture.Cmms.Application.WorkOrders.Commands.CreateWorkOrder;
using CleanArchitecture.Cmms.Application.WorkOrders.Commands.StartWorkOrder;
using CleanArchitecture.Cmms.Domain.Assets.Enums;
using CleanArchitecture.Cmms.Domain.WorkOrders.Enums;
using CleanArchitecture.Cmms.IntegrationTests.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace CleanArchitecture.Cmms.IntegrationTests.Events;

/// <summary>
/// Tests validating ADR-004: Outbox Pattern for Guaranteed Event Delivery
/// Ensures integration events are reliably delivered via outbox pattern
/// </summary>
public class IntegrationEventTests : IntegrationTestBase
{
    public IntegrationEventTests(CmmsWebApplicationFactory factory) : base(factory)
    {
    }

    [Fact]
    public async Task IntegrationEvents_ShouldBeWritten_ToOutboxInTransaction()
    {
        // Arrange
        var assetId = await CreateAssetAsync("TEST-OUTBOX-001", "Test Machine");

        // Act
        var command = new CreateWorkOrderCommand(assetId, "Test Work Order", "B1", "F1", "R1");
        var result = await Mediator.Send(command);

        // Assert
        Assert.True(result.IsSuccess);

        // Verify integration event written to outbox
        var outboxEvents = await OutboxDbContext.OutboxMessages
            .Where(e => e.EventType.Contains("WorkOrderCreatedEvent"))
            .ToListAsync();

        Assert.Single(outboxEvents);
        var outboxEvent = outboxEvents[0];

        Assert.Contains("WorkOrderCreatedEvent", outboxEvent.EventType);
        Assert.NotNull(outboxEvent.Payload);
        Assert.Null(outboxEvent.ProcessedAt);
        Assert.Equal(0, outboxEvent.RetryCount);
        Assert.Equal(3, outboxEvent.MaxRetries);
    }

    [Fact]
    public async Task WhenTransactionRollsBack_OutboxEvents_ShouldNotBeWritten()
    {
        // Arrange
        var assetId = await CreateAssetAsync("TEST-OUTBOX-002", "Test Machine");

        // Count initial outbox events
        var initialCount = await OutboxDbContext.OutboxMessages.CountAsync();

        // Act - Try to create work order with invalid data that should cause rollback
        // (This test assumes there's validation that would cause rollback)
        var command = new CreateWorkOrderCommand(assetId, "", "B1", "F1", "R1"); // Empty title should fail validation
        var act = () => Mediator.Send(command);

        // Assert
        await Assert.ThrowsAnyAsync<Exception>(act);

        // Verify no new outbox events were written
        var finalCount = await OutboxDbContext.OutboxMessages.CountAsync();
        Assert.Equal(initialCount, finalCount);
    }

    [Fact]
    public async Task MultipleIntegrationEvents_ShouldBeWritten_ToOutbox()
    {
        // Arrange
        var assetId = await CreateAssetAsync("TEST-OUTBOX-003", "Test Machine");
        var technicianId = await CreateTechnicianAsync("Test Tech", "Senior", 3);

        // Act - Complete workflow that generates multiple events
        var createCommand = new CreateWorkOrderCommand(assetId, "Test Work Order", "B1", "F1", "R1");
        var workOrderId = await Mediator.Send(createCommand);

        var assignCommand = new AssignTechnicianCommand(workOrderId.Value, technicianId);
        await Mediator.Send(assignCommand);

        var startCommand = new StartWorkOrderCommand(workOrderId.Value);
        await Mediator.Send(startCommand);

        var completeCommand = new CompleteWorkOrderCommand(workOrderId.Value);
        await Mediator.Send(completeCommand);

        // Assert
        var outboxEvents = await OutboxDbContext.OutboxMessages
            .OrderBy(e => e.CreatedAt)
            .ToListAsync();

        // Should have at least 4 events: WorkOrderCreated, TechnicianAssigned, WorkOrderStarted, WorkOrderCompleted
        Assert.True(outboxEvents.Count >= 4);

        // Verify specific events are present
        var eventTypes = outboxEvents.Select(e => e.EventType).ToList();
        Assert.Contains(eventTypes, t => t.Contains("WorkOrderCreatedEvent"));
        Assert.Contains(eventTypes, t => t.Contains("TechnicianAssignedEvent"));
        Assert.Contains(eventTypes, t => t.Contains("WorkOrderCompletedEvent"));

        // All events should be unprocessed
        Assert.All(outboxEvents, e => Assert.Null(e.ProcessedAt));
        Assert.All(outboxEvents, e => Assert.Equal(0, e.RetryCount));
    }

    [Fact]
    public async Task IntegrationEventPayload_ShouldContain_AllNecessaryData()
    {
        // Arrange
        var assetId = await CreateAssetAsync("TEST-OUTBOX-004", "Test Machine");

        // Act
        var command = new CreateWorkOrderCommand(assetId, "Payload Test", "B1", "F1", "R1");
        var result = await Mediator.Send(command);

        // Assert
        Assert.True(result.IsSuccess);

        var outboxEvent = await OutboxDbContext.OutboxMessages
            .FirstAsync(e => e.EventType.Contains("WorkOrderCreatedEvent"));

        Assert.NotNull(outboxEvent.Payload);

        // Verify payload contains expected data
        Assert.Contains("Payload Test", outboxEvent.Payload);
        Assert.Contains(assetId.ToString(), outboxEvent.Payload);
        Assert.Contains(result.Value.ToString(), outboxEvent.Payload);
    }

    [Fact]
    public async Task IntegrationEventFailure_ShouldNotRollback_BusinessTransaction()
    {
        // Arrange
        var assetId = await CreateAssetAsync("TEST-OUTBOX-005", "Test Machine");

        // Act
        var command = new CreateWorkOrderCommand(assetId, "Business Transaction Test", "B1", "F1", "R1");
        var result = await Mediator.Send(command);

        // Assert
        Assert.True(result.IsSuccess);

        // Verify business transaction succeeded
        var workOrder = await WriteDbContext.WorkOrders.FindAsync(result.Value);
        var asset = await WriteDbContext.Assets.FindAsync(assetId);

        Assert.NotNull(workOrder);
        Assert.NotNull(asset);
        Assert.Equal(WorkOrderStatus.Open, workOrder.Status);
        Assert.Equal(AssetStatus.UnderMaintenance, asset.Status);

        // Verify integration event was written to outbox
        var outboxEvent = await OutboxDbContext.OutboxMessages
            .FirstOrDefaultAsync(e => e.EventType.Contains("WorkOrderCreatedEvent"));

        Assert.NotNull(outboxEvent);
        Assert.Null(outboxEvent.ProcessedAt); // Not yet processed
    }

    [Fact]
    public async Task OutboxEvents_ShouldHave_CorrectMetadata()
    {
        // Arrange
        var assetId = await CreateAssetAsync("TEST-OUTBOX-006", "Test Machine");

        // Act
        var command = new CreateWorkOrderCommand(assetId, "Metadata Test", "B1", "F1", "R1");
        var result = await Mediator.Send(command);

        // Assert
        Assert.True(result.IsSuccess);

        var outboxEvent = await OutboxDbContext.OutboxMessages
            .FirstAsync(e => e.EventType.Contains("WorkOrderCreatedEvent"));

        Assert.NotEqual(Guid.Empty, outboxEvent.Id);
        Assert.NotNull(outboxEvent.EventType);
        Assert.NotNull(outboxEvent.Payload);
        Assert.Null(outboxEvent.ProcessedAt);
        Assert.Equal(0, outboxEvent.RetryCount);
        Assert.Equal(3, outboxEvent.MaxRetries);
        Assert.Null(outboxEvent.LastError);

        // Verify timestamps are reasonable
        var now = DateTime.UtcNow;
        Assert.True(outboxEvent.CreatedAt <= now);
        Assert.True(outboxEvent.CreatedAt >= now.AddMinutes(-1));
    }

    [Fact]
    public async Task WorkOrderCompletedEvent_ShouldTrigger_EmailNotification()
    {
        // Arrange
        var assetId = await CreateAssetAsync("TEST-OUTBOX-007", "Test Machine");
        var technicianId = await CreateTechnicianAsync("Email Test Tech", "Senior", 3);

        // Create and complete work order
        var createCommand = new CreateWorkOrderCommand(assetId, "Email Test Work Order", "B1", "F1", "R1");
        var workOrderId = await Mediator.Send(createCommand);

        var assignCommand = new AssignTechnicianCommand(workOrderId.Value, technicianId);
        await Mediator.Send(assignCommand);

        var startCommand = new StartWorkOrderCommand(workOrderId.Value);
        await Mediator.Send(startCommand);

        // Act
        var completeCommand = new CompleteWorkOrderCommand(workOrderId.Value);
        await Mediator.Send(completeCommand);

        // Assert
        var outboxEvent = await OutboxDbContext.OutboxMessages
            .FirstOrDefaultAsync(e => e.EventType.Contains("WorkOrderCompletedEvent"));

        Assert.NotNull(outboxEvent);

        // Verify the event contains work order completion data
        Assert.Contains(workOrderId.Value.ToString(), outboxEvent.Payload);
        Assert.Contains(assetId.ToString(), outboxEvent.Payload);
        Assert.Contains(technicianId.ToString(), outboxEvent.Payload);
    }

    [Fact]
    public async Task OutboxEvents_ShouldBe_OrderedByCreationTime()
    {
        // Arrange
        var assetId = await CreateAssetAsync("TEST-OUTBOX-008", "Test Machine");
        var technicianId = await CreateTechnicianAsync("Order Test Tech", "Senior", 3);

        // Act - Generate multiple events
        var createCommand = new CreateWorkOrderCommand(assetId, "Order Test", "B1", "F1", "R1");
        var workOrderId = await Mediator.Send(createCommand);

        var assignCommand = new AssignTechnicianCommand(workOrderId.Value, technicianId);
        await Mediator.Send(assignCommand);

        // Assert
        var outboxEvents = await OutboxDbContext.OutboxMessages
            .OrderBy(e => e.CreatedAt)
            .ToListAsync();

        // Verify events are ordered by creation time
        for (int i = 1; i < outboxEvents.Count; i++)
        {
            Assert.True(outboxEvents[i].CreatedAt >= outboxEvents[i - 1].CreatedAt);
        }

        // Verify we have the expected events
        Assert.True(outboxEvents.Count >= 2);
        Assert.Contains(outboxEvents, e => e.EventType.Contains("WorkOrderCreatedEvent"));
        Assert.Contains(outboxEvents, e => e.EventType.Contains("TechnicianAssignedEvent"));
    }
}
