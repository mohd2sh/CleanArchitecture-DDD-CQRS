using CleanArchitecture.Cmms.Domain.Technicians.Events;

namespace CleanArchitecture.Cmms.Domain.UnitTests.Technicians.Events
{
    public class TechnicianCertificationAddedEventTests
    {
        [Fact]
        public void Ctor_Should_Set_Properties_And_Default_Null_Timestamp()
        {
            // Arrange
            var technicianId = Guid.NewGuid();
            var certificationCode = "ELEC-1";

            // Act
            var domainEvent = new TechnicianCertificationAddedEvent(technicianId, certificationCode);

            // Assert
            Assert.Equal(technicianId, domainEvent.TechnicianId);
            Assert.Equal(certificationCode, domainEvent.CertificationCode);
            Assert.Null(domainEvent.OccurredOn);
        }

        [Fact]
        public void Ctor_Should_Respect_Provided_Timestamp()
        {
            // Arrange
            var technicianId = Guid.NewGuid();
            var certificationCode = "HVAC-2";
            var providedTimestamp = DateTime.UtcNow;

            // Act
            var domainEvent = new TechnicianCertificationAddedEvent(technicianId, certificationCode, providedTimestamp);

            // Assert
            Assert.Equal(providedTimestamp, domainEvent.OccurredOn);
        }
    }
}
