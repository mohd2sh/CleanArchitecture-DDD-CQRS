using CleanArchitecture.Cmms.Domain.Assets.Enums;
using CleanArchitecture.Cmms.Domain.Assets.Events;

namespace CleanArchitecture.Cmms.Domain.UnitTests.Assets.Events
{
    public class AssetStatusChangedEventTests
    {
        [Fact]
        public void Ctor_Should_Assign_Properties_With_Default_Null_Timestamp()
        {
            // Arrange
            var assetId = Guid.NewGuid();
            var newStatus = AssetStatus.Inactive;

            // Act
            var domainEvent = new AssetStatusChangedEvent(assetId, newStatus);

            // Assert
            Assert.Equal(assetId, domainEvent.AssetId);
            Assert.Equal(newStatus, domainEvent.NewStatus);
            Assert.Null(domainEvent.OccurredOn);
        }

        [Fact]
        public void Ctor_Should_Respect_Provided_Timestamp()
        {
            // Arrange
            var assetId = Guid.NewGuid();
            var newStatus = AssetStatus.Decommissioned;
            var providedTimestamp = DateTime.UtcNow;

            // Act
            var domainEvent = new AssetStatusChangedEvent(assetId, newStatus, providedTimestamp);

            // Assert
            Assert.Equal(providedTimestamp, domainEvent.OccurredOn);
        }
    }
}
