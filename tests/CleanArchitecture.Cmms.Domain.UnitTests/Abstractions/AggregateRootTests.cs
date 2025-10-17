using CleanArchitecture.Cmms.Domain.Abstractions;
using CleanArchitecture.Cmms.Domain.Abstractions;

namespace CleanArchitecture.Cmms.Domain.UnitTests.Abstractions
{
    public class AggregateRootTests
    {

        [Fact]
        public void Constructor_WithId_Should_Set_Id()
        {
            // Arrange
            var expectedId = Guid.NewGuid();

            // Act
            var aggregateRoot = new TestAggregateRoot(expectedId);

            // Assert
            Assert.Equal(expectedId, aggregateRoot.Id);
        }

        [Fact]
        public void Parameterless_Constructor_Should_Create_Instance()
        {
            // Arrange & Act
            var aggregateRoot = new TestAggregateRoot();

            // Assert
            Assert.NotNull(aggregateRoot);
        }

        [Fact]
        public void AggregateRoot_Should_Be_Entity_And_IAggregateRoot()
        {
            // Arrange
            var aggregateRoot = new TestAggregateRoot(Guid.NewGuid());

            // Act & Assert
            Assert.IsAssignableFrom<Entity<Guid>>(aggregateRoot);
            Assert.IsAssignableFrom<IAggregateRoot>(aggregateRoot);
        }

    }

    internal class TestAggregateRoot : AggregateRoot<Guid>
    {
        public TestAggregateRoot() : base() { }

        public TestAggregateRoot(Guid id) : base(id) { }
    }
}
