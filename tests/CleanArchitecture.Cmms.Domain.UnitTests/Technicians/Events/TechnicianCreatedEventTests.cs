using CleanArchitecture.Cmms.Domain.Technicians.Events;

namespace CleanArchitecture.Cmms.Domain.UnitTests.Technicians.Events
{
    public class TechnicianCreatedEventTests
    {
        [Fact]
        public void Ctor_Should_Set_Properties_And_Default_Null_Timestamp()
        {
            // Arrange
            var technicianId = Guid.NewGuid();
            var technicianName = "John";
            var skillLevelName = "Master";

            // Act
            var domainEvent = new TechnicianCreatedEvent(technicianId, technicianName, skillLevelName);

            // Assert
            Assert.Equal(technicianId, domainEvent.TechnicianId);
            Assert.Equal(technicianName, domainEvent.Name);
            Assert.Equal(skillLevelName, domainEvent.SkillLevelName);
            Assert.Null(domainEvent.OccurredOn);
        }

        [Fact]
        public void Ctor_Should_Respect_Provided_Timestamp()
        {
            // Arrange
            var technicianId = Guid.NewGuid();
            var technicianName = "Jane";
            var skillLevelName = "Journeyman";
            var providedTimestamp = DateTime.UtcNow;

            // Act
            var domainEvent = new TechnicianCreatedEvent(technicianId, technicianName, skillLevelName, providedTimestamp);

            // Assert
            Assert.Equal(providedTimestamp, domainEvent.OccurredOn);
        }
    }
}
