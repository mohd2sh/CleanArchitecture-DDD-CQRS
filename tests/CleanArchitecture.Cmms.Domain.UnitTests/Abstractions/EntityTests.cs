using CleanArchitecture.Core.Domain.Abstractions;

namespace CleanArchitecture.Cmms.Domain.UnitTests.Abstractions
{
    public class EntityTests
    {
        [Fact]
        public void Constructor_WithId_Should_Set_Id()
        {
            // Arrange
            var id = Guid.NewGuid();

            // Act
            var entity = new TestEntity(id);

            // Assert
            Assert.Equal(id, entity.Id);
        }

        [Fact]
        public void Default_Constructor_Should_Initialize_Id_To_Default()
        {
            // Arrange & Act
            var entity = new TestEntity();

            // Assert
            Assert.Equal(Guid.Empty, entity.Id);
        }

        [Fact]
        public void Raise_Should_Add_DomainEvent()
        {
            // Arrange
            var entity = new TestEntity(Guid.NewGuid());
            var domainEvent = new TestDomainEvent("Created");

            // Act
            entity.RaiseEvent(domainEvent);

            // Assert
            Assert.Single(entity.DomainEvents);
            Assert.Equal(domainEvent, entity.DomainEvents.First());
        }

        [Fact]
        public void DomainEvents_Should_Be_ReadOnly()
        {
            // Arrange
            var entity = new TestEntity(Guid.NewGuid());
            var domainEvent = new TestDomainEvent("Created");
            entity.RaiseEvent(domainEvent);

            // Act
            var events = entity.DomainEvents;

            // Assert
            Assert.IsAssignableFrom<IReadOnlyCollection<IDomainEvent>>(events);
            Assert.True(events.Count == 1);
        }

        [Fact]
        public void ClearDomainEvents_Should_Remove_All_Events()
        {
            // Arrange
            var entity = new TestEntity(Guid.NewGuid());
            entity.RaiseEvent(new TestDomainEvent("Created"));
            entity.RaiseEvent(new TestDomainEvent("Updated"));

            // Act
            entity.ClearDomainEvents();

            // Assert
            Assert.Empty(entity.DomainEvents);
        }
    }

    internal sealed class TestDomainEvent : IDomainEvent
    {
        public string Name { get; }

        public DateTime? OccurredOn => DateTime.UtcNow;

        public TestDomainEvent(string name) => Name = name;
    }

    internal sealed class TestEntity : Entity<Guid>
    {
        public TestEntity() : base() { }

        public TestEntity(Guid id) : base(id) { }

        public void RaiseEvent(IDomainEvent e) => Raise(e);
    }
}
