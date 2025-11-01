using CleanArchitecture.Cmms.Domain.WorkOrders.Events;

namespace CleanArchitecture.Cmms.Domain.UnitTests.WorkOrders.Events;

public class WorkOrderCompletedEventTests
{
    [Fact]
    public void Ctor_Should_Set_Properties_And_Timestamp()
    {
        // Arrange
        var id = Guid.NewGuid();
        var assetId = Guid.NewGuid();
        var techId = Guid.NewGuid();

        // Act
        var evt = new WorkOrderCompletedEvent(id, assetId, techId);

        // Assert
        Assert.Equal(id, evt.WorkOrderId);
        Assert.Equal(assetId, evt.AssetId);
        Assert.Equal(techId, evt.TechnicianId);
        Assert.NotNull(evt.OccurredOn);
    }
}
