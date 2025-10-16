using CleanArchitecture.Cmms.Domain.Abstractions;

namespace CleanArchitecture.Cmms.Domain.UnitTests.Abstractions
{
    public class DomainExceptionTests
    {
        [Fact]
        public void Constructor_WithMessage_Should_Set_Message()
        {
            // Arrange
            string expectedMessage = "Something went wrong";

            // Act
            var exception = new DomainException(expectedMessage);

            // Assert
            Assert.Equal(expectedMessage, exception.Message);
            Assert.Null(exception.InnerException);
        }

        [Fact]
        public void Constructor_WithMessage_And_InnerException_Should_Set_Both()
        {
            // Arrange
            string expectedMessage = "Outer error";
            var inner = new InvalidOperationException("Inner cause");

            // Act
            var exception = new DomainException(expectedMessage, inner);

            // Assert
            Assert.Equal(expectedMessage, exception.Message);
            Assert.Equal(inner, exception.InnerException);
        }
    }
}
