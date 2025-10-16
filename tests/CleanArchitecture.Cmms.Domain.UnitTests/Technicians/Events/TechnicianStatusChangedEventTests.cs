using CleanArchitecture.Cmms.Domain.Technicians.Enums;
using CleanArchitecture.Cmms.Domain.Technicians.Events;

namespace CleanArchitecture.Cmms.Domain.UnitTests.Technicians.Events
{
    public class TechnicianStatusChangedEventTests
    {
        [Fact]
        public void Ctor_Should_Set_Properties_And_Default_Null_Timestamp()
        {
            // Arrange
            Guid technicianId = Guid.NewGuid();
            TechnicianStatus newStatus = TechnicianStatus.Unavailable;

            // Act
            TechnicianStatusChangedEvent domainEvent = new TechnicianStatusChangedEvent(technicianId, newStatus);

            // Assert
            Assert.Equal(technicianId, domainEvent.TechnicianId);
            Assert.Equal(newStatus, domainEvent.NewStatus);
            Assert.Null(domainEvent.OccurredOn);
        }

        [Fact]
        public void Ctor_Should_Respect_Provided_Timestamp()
        {
            // Arrange
            Guid technicianId = Guid.NewGuid();
            TechnicianStatus newStatus = TechnicianStatus.Available;
            DateTime providedTimestamp = DateTime.UtcNow;

            // Act
            TechnicianStatusChangedEvent domainEvent = new TechnicianStatusChangedEvent(technicianId, newStatus, providedTimestamp);

            // Assert
            Assert.Equal(providedTimestamp, domainEvent.OccurredOn);
        }
    }
}
