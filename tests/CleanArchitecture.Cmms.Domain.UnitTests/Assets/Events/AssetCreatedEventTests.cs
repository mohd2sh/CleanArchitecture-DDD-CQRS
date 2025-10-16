using CleanArchitecture.Cmms.Domain.Assets.Events;

namespace CleanArchitecture.Cmms.Domain.UnitTests.Assets.Events
{
    public class AssetCreatedEventTests
    {
        [Fact]
        public void Ctor_Should_Assign_Properties_With_Default_Null_Timestamp()
        {
            // Arrange
            var assetId = Guid.NewGuid();
            var name = "Pump";
            var type = "Mechanical";
            var tagValue = "A-1";

            // Act
            var domainEvent = new AssetCreatedEvent(assetId, name, type, tagValue);

            // Assert
            Assert.Equal(assetId, domainEvent.AssetId);
            Assert.Equal(name, domainEvent.Name);
            Assert.Equal(type, domainEvent.Type);
            Assert.Equal(tagValue, domainEvent.TagValue);
            Assert.Null(domainEvent.OccurredOn);
        }

        [Fact]
        public void Ctor_Should_Respect_Provided_Timestamp()
        {
            // Arrange
            var assetId = Guid.NewGuid();
            var name = "Pump";
            var type = "Mechanical";
            var tagValue = "A-1";
            var providedTimestamp = DateTime.UtcNow;

            // Act
            var domainEvent = new AssetCreatedEvent(assetId, name, type, tagValue, providedTimestamp);

            // Assert
            Assert.Equal(providedTimestamp, domainEvent.OccurredOn);
        }
    }
}
