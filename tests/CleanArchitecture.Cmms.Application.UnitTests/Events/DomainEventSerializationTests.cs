using System.Text.Json;
using CleanArchitecture.Cmms.Domain.WorkOrders.Events;

namespace CleanArchitecture.Cmms.Application.UnitTests.Events;

public class DomainEventSerializationTests
{
    private static readonly JsonSerializerOptions Options = new()
    {
        PropertyNameCaseInsensitive = true,
        IncludeFields = false
    };

    [Fact]
    public void WorkOrderCompletedEvent_Should_SerializeAndDeserialize()
    {
        // Arrange
        var original = new WorkOrderCompletedEvent(
            Guid.NewGuid(),
            Guid.NewGuid(),
            Guid.NewGuid());

        // Act
        var json = JsonSerializer.Serialize(original, Options);
        var deserialized = JsonSerializer.Deserialize<WorkOrderCompletedEvent>(json, Options);

        // Assert
        deserialized.Should().NotBeNull();
        deserialized!.WorkOrderId.Should().Be(original.WorkOrderId);
        deserialized.AssetId.Should().Be(original.AssetId);
        deserialized.TechnicianId.Should().Be(original.TechnicianId);
    }

    [Fact]
    public void WorkOrderCreatedEvent_Should_SerializeAndDeserialize()
    {
        // Arrange
        var original = new WorkOrderCreatedEvent(
            Guid.NewGuid(),
            Guid.NewGuid(),
            "Test Title");

        // Act
        var json = JsonSerializer.Serialize(original, Options);
        var deserialized = JsonSerializer.Deserialize<WorkOrderCreatedEvent>(json, Options);

        // Assert
        deserialized.Should().NotBeNull();
        deserialized!.WorkOrderId.Should().Be(original.WorkOrderId);
        deserialized.AssetId.Should().Be(original.AssetId);
        deserialized.Title.Should().Be(original.Title);
    }

    [Fact]
    public void TechnicianAssignedEvent_Should_SerializeAndDeserialize()
    {
        // Arrange
        var original = new TechnicianAssignedEvent(
            Guid.NewGuid(),
            Guid.NewGuid());

        // Act
        var json = JsonSerializer.Serialize(original, Options);
        var deserialized = JsonSerializer.Deserialize<TechnicianAssignedEvent>(json, Options);

        // Assert
        deserialized.Should().NotBeNull();
        deserialized!.WorkOrderId.Should().Be(original.WorkOrderId);
        deserialized.TechnicianId.Should().Be(original.TechnicianId);
    }
}
