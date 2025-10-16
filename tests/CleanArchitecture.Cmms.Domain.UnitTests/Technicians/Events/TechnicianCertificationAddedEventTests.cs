using CleanArchitecture.Cmms.Domain.Technicians.Events;

namespace CleanArchitecture.Cmms.Domain.UnitTests.Technicians.Events
{
    public class TechnicianCertificationAddedEventTests
    {
        [Fact]
        public void Ctor_Should_Set_Properties_And_Default_Null_Timestamp()
        {
            // Arrange
            Guid technicianId = Guid.NewGuid();
            string certificationCode = "ELEC-1";

            // Act
            TechnicianCertificationAddedEvent domainEvent = new TechnicianCertificationAddedEvent(technicianId, certificationCode);

            // Assert
            Assert.Equal(technicianId, domainEvent.TechnicianId);
            Assert.Equal(certificationCode, domainEvent.CertificationCode);
            Assert.Null(domainEvent.OccurredOn);
        }

        [Fact]
        public void Ctor_Should_Respect_Provided_Timestamp()
        {
            // Arrange
            Guid technicianId = Guid.NewGuid();
            string certificationCode = "HVAC-2";
            DateTime providedTimestamp = DateTime.UtcNow;

            // Act
            TechnicianCertificationAddedEvent domainEvent = new TechnicianCertificationAddedEvent(technicianId, certificationCode, providedTimestamp);

            // Assert
            Assert.Equal(providedTimestamp, domainEvent.OccurredOn);
        }
    }
}
