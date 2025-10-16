using CleanArchitecture.Cmms.Domain.Technicians.Events;

namespace CleanArchitecture.Cmms.Domain.UnitTests.Technicians.Events
{
    public class TechnicianUnassignedFromWorkOrderEventTests
    {
        [Fact]
        public void Ctor_Should_Set_Properties_And_Default_Null_Timestamp()
        {
            // Arrange
            Guid technicianId = Guid.NewGuid();
            Guid workOrderId = Guid.NewGuid();

            // Act
            TechnicianUnassignedFromWorkOrderEvent domainEvent = new TechnicianUnassignedFromWorkOrderEvent(technicianId, workOrderId);

            // Assert
            Assert.Equal(technicianId, domainEvent.TechnicianId);
            Assert.Equal(workOrderId, domainEvent.WorkOrderId);
            Assert.Null(domainEvent.OccurredOn);
        }

        [Fact]
        public void Ctor_Should_Respect_Provided_Timestamp()
        {
            // Arrange
            Guid technicianId = Guid.NewGuid();
            Guid workOrderId = Guid.NewGuid();
            DateTime providedTimestamp = DateTime.UtcNow;

            // Act
            TechnicianUnassignedFromWorkOrderEvent domainEvent = new TechnicianUnassignedFromWorkOrderEvent(technicianId, workOrderId, providedTimestamp);

            // Assert
            Assert.Equal(providedTimestamp, domainEvent.OccurredOn);
        }
    }
}
