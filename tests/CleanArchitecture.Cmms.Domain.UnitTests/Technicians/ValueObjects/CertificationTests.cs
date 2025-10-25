using CleanArchitecture.Cmms.Domain.Technicians.ValueObjects;

namespace CleanArchitecture.Cmms.Domain.UnitTests.Technicians.ValueObjects
{
    public class CertificationTests
    {
        [Fact]
        public void IsValid_Should_Return_True_When_No_Expiry()
        {
            // Arrange
            var certification = Certification.Create("ELEC-1", DateTime.UtcNow.AddYears(-1), null);
            var nowUtc = DateTime.UtcNow;

            // Act
            var isValid = certification.IsValid(nowUtc);

            // Assert
            Assert.True(isValid);
        }

        [Fact]
        public void IsValid_Should_Return_True_When_Expiry_In_Future()
        {
            // Arrange
            var certification = Certification.Create("ELEC-1", DateTime.UtcNow.AddYears(-1), DateTime.UtcNow.AddDays(10));
            var nowUtc = DateTime.UtcNow;

            // Act
            var isValid = certification.IsValid(nowUtc);

            // Assert
            Assert.True(isValid);
        }

        [Fact]
        public void IsValid_Should_Return_False_When_Expired()
        {
            // Arrange
            var certification = Certification.Create("ELEC-1", DateTime.UtcNow.AddYears(-2), DateTime.UtcNow.AddDays(-1));
            var nowUtc = DateTime.UtcNow;

            // Act
            var isValid = certification.IsValid(nowUtc);

            // Assert
            Assert.False(isValid);
        }

        [Fact]
        public void Equality_Should_Be_Value_Based()
        {
            // Arrange
            var first = Certification.Create("ELEC-1", new DateTime(2024, 1, 1), new DateTime(2026, 1, 1));
            var second = Certification.Create("ELEC-1", new DateTime(2024, 1, 1), new DateTime(2026, 1, 1));

            // Act
            var areEqual = first == second;

            // Assert
            Assert.True(areEqual);
        }

        [Fact]
        public void ToString_Should_Format_Code_And_IssuedOn()
        {
            // Arrange
            var certification = Certification.Create("ELEC-1", new DateTime(2024, 5, 10), null);

            // Act
            var text = certification.ToString();

            // Assert
            Assert.Equal("ELEC-1 (Issued: 2024-05-10)", text);
        }
    }
}
