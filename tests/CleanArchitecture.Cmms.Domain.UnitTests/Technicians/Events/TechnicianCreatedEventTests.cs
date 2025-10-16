using CleanArchitecture.Cmms.Domain.Technicians.Events;

namespace CleanArchitecture.Cmms.Domain.UnitTests.Technicians.Events
{
    public class TechnicianCreatedEventTests
    {
        [Fact]
        public void Ctor_Should_Set_Properties_And_Default_Null_Timestamp()
        {
            // Arrange
            Guid technicianId = Guid.NewGuid();
            string technicianName = "John";
            string skillLevelName = "Master";

            // Act
            TechnicianCreatedEvent domainEvent = new TechnicianCreatedEvent(technicianId, technicianName, skillLevelName);

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
            Guid technicianId = Guid.NewGuid();
            string technicianName = "Jane";
            string skillLevelName = "Journeyman";
            DateTime providedTimestamp = DateTime.UtcNow;

            // Act
            TechnicianCreatedEvent domainEvent = new TechnicianCreatedEvent(technicianId, technicianName, skillLevelName, providedTimestamp);

            // Assert
            Assert.Equal(providedTimestamp, domainEvent.OccurredOn);
        }
    }
}
