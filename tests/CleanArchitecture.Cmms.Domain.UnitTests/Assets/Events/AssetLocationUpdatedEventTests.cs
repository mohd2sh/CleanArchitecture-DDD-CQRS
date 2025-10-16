using CleanArchitecture.Cmms.Domain.Assets.Events;

namespace CleanArchitecture.Cmms.Domain.UnitTests.Assets.Events
{
    public class AssetLocationUpdatedEventTests
    {
        [Fact]
        public void Ctor_Should_Assign_Properties_With_Default_Null_Timestamp()
        {
            // Arrange
            var assetId = Guid.NewGuid();

            // Act
            var domainEvent = new AssetLocationUpdatedEvent(assetId);

            // Assert
            Assert.Equal(assetId, domainEvent.AssetId);
            Assert.Null(domainEvent.OccurredOn);
        }

        [Fact]
        public void Ctor_Should_Respect_Provided_Timestamp()
        {
            // Arrange
            var assetId = Guid.NewGuid();
            var providedTimestamp = DateTime.UtcNow;

            // Act
            var domainEvent = new AssetLocationUpdatedEvent(assetId, providedTimestamp);

            // Assert
            Assert.Equal(providedTimestamp, domainEvent.OccurredOn);
        }
    }
}
