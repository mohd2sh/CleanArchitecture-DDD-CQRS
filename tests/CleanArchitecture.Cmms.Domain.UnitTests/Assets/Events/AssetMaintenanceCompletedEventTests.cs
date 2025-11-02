using CleanArchitecture.Cmms.Domain.Assets.Events;

namespace CleanArchitecture.Cmms.Domain.UnitTests.Assets.Events;

public class AssetMaintenanceCompletedEventTests
{
    [Fact]
    public void Ctor_Should_Assign_Properties_With_Default_Null_Timestamp()
    {
        // Arrange
        var assetId = Guid.NewGuid();
        var completedOn = DateTime.UtcNow;
        var notes = "Completed";

        // Act
        var domainEvent = new AssetMaintenanceCompletedEvent(assetId, completedOn, notes);

        // Assert
        Assert.Equal(assetId, domainEvent.AssetId);
        Assert.Equal(completedOn, domainEvent.CompletedOn);
        Assert.Equal(notes, domainEvent.Notes);
        Assert.Null(domainEvent.OccurredOn);
    }

    [Fact]
    public void Ctor_Should_Respect_Provided_Timestamp()
    {
        // Arrange
        var assetId = Guid.NewGuid();
        var completedOn = DateTime.UtcNow;
        var notes = "Completed";
        var providedTimestamp = DateTime.UtcNow;

        // Act
        var domainEvent = new AssetMaintenanceCompletedEvent(assetId, completedOn, notes, providedTimestamp);

        // Assert
        Assert.Equal(providedTimestamp, domainEvent.OccurredOn);
    }
}
