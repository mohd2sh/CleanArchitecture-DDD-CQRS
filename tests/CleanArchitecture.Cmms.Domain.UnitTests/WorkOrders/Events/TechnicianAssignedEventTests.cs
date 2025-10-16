using CleanArchitecture.Cmms.Domain.WorkOrders.Events;

namespace CleanArchitecture.Cmms.Domain.UnitTests.WorkOrders.Events
{
    public class TechnicianAssignedEventTests
    {
        [Fact]
        public void Ctor_Should_Set_Properties_And_Timestamp()
        {
            // Arrange
            Guid woId = Guid.NewGuid();
            Guid techId = Guid.NewGuid();

            // Act
            TechnicianAssignedEvent evt = new TechnicianAssignedEvent(woId, techId);

            // Assert
            Assert.Equal(woId, evt.WorkOrderId);
            Assert.Equal(techId, evt.TechnicianId);
            Assert.NotNull(evt.OccurredOn);
        }
    }
}
