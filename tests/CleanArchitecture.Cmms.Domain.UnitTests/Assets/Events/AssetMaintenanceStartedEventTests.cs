using CleanArchitecture.Cmms.Domain.Assets.Events;

namespace CleanArchitecture.Cmms.Domain.UnitTests.Assets.Events;

public class AssetMaintenanceStartedEventTests
{
    [Fact]
    public void Ctor_Should_Assign_All_Properties_With_Default_Null_Timestamp()
    {
        // Arrange
        var assetId = Guid.NewGuid();
        var maintenanceRecordId = Guid.NewGuid();
        var startedOn = DateTime.UtcNow;
        var description = "Desc";
        var performedBy = "Tech";

        // Act
        var domainEvent = new AssetMaintenanceStartedEvent(assetId, maintenanceRecordId, startedOn, description, performedBy);

        // Assert
        Assert.Equal(assetId, domainEvent.AssetId);
        Assert.Equal(maintenanceRecordId, domainEvent.MaintenanceRecordId);
        Assert.Equal(startedOn, domainEvent.StartedOn);
        Assert.Equal(description, domainEvent.Description);
        Assert.Equal(performedBy, domainEvent.PerformedBy);
        Assert.Null(domainEvent.OccurredOn);
    }

    [Fact]
    public void Ctor_Should_Respect_Provided_Timestamp()
    {
        // Arrange
        var assetId = Guid.NewGuid();
        var maintenanceRecordId = Guid.NewGuid();
        var startedOn = DateTime.UtcNow.AddMinutes(-1);
        var description = "Desc";
        var performedBy = "Tech";
        var providedTimestamp = DateTime.UtcNow;

        // Act
        var domainEvent = new AssetMaintenanceStartedEvent(assetId, maintenanceRecordId, startedOn, description, performedBy, providedTimestamp);

        // Assert
        Assert.Equal(providedTimestamp, domainEvent.OccurredOn);
    }
}
