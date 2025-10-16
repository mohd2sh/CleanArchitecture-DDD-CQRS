using CleanArchitecture.Cmms.Domain.WorkOrders.Events;

namespace CleanArchitecture.Cmms.Domain.UnitTests.WorkOrders.Events
{
    public class WorkOrderCreatedEventTests
    {
        [Fact]
        public void Ctor_Should_Set_Properties_And_Timestamp()
        {
            // Arrange
            Guid id = Guid.NewGuid();
            Guid assetId = Guid.NewGuid();
            string title = "Title";

            // Act
            WorkOrderCreatedEvent evt = new WorkOrderCreatedEvent(id, assetId, title);

            // Assert
            Assert.Equal(id, evt.WorkOrderId);
            Assert.Equal(assetId, evt.AssetId);
            Assert.Equal(title, evt.Title);
            Assert.NotNull(evt.OccurredOn);
        }
    }
}
