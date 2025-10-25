using CleanArchitecture.Core.Domain.Abstractions;

namespace CleanArchitecture.Cmms.Domain.UnitTests.Abstractions
{
    public class DomainExceptionTests
    {
        [Fact]
        public void Constructor_WithDomainError_Should_Set_Error()
        {
            // Arrange
            var domainError = DomainError.Create("Test.Error", "Something went wrong");

            // Act
            var exception = new DomainException(domainError);

            // Assert
            Assert.Equal(domainError.Message, exception.Message);
            Assert.Equal(domainError, exception.Error);
            Assert.Null(exception.InnerException);
        }

        [Fact]
        public void Constructor_WithDomainError_And_InnerException_Should_Set_Both()
        {
            // Arrange
            var domainError = DomainError.Create("Test.Error", "Outer error");
            var inner = new InvalidOperationException("Inner cause");

            // Act
            var exception = new DomainException(domainError, inner);

            // Assert
            Assert.Equal(domainError.Message, exception.Message);
            Assert.Equal(domainError, exception.Error);
            Assert.Equal(inner, exception.InnerException);
        }
    }
}
