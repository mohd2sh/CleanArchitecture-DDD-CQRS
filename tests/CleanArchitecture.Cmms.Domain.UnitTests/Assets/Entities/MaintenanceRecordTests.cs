using CleanArchitecture.Cmms.Domain.Abstractions;
using CleanArchitecture.Cmms.Domain.Assets.Entities;

namespace CleanArchitecture.Cmms.Domain.UnitTests.Assets.Entities
{
    public class MaintenanceRecordTests
    {
        [Fact]
        public void Create_Should_Set_All_Properties()
        {
            // Arrange
            var assetId = Guid.NewGuid();
            var startedOn = DateTime.UtcNow;
            var description = "Filter Replacement";
            var performedBy = "Technician X";

            // Act
            var record = MaintenanceRecord.Create(assetId, startedOn, description, performedBy);

            // Assert
            Assert.Equal(assetId, record.AssetId);
            Assert.Equal(startedOn, record.StartedOn);
            Assert.Equal(description, record.Description);
            Assert.Equal(performedBy, record.PerformedBy);
        }

        [Fact]
        public void Create_Should_Throw_When_Description_Is_Empty()
        {
            // Arrange
            var assetId = Guid.NewGuid();
            var startedOn = DateTime.UtcNow;
            var emptyDescription = " ";
            var performedBy = "Tech";

            // Act
            void act() => MaintenanceRecord.Create(assetId, startedOn, emptyDescription, performedBy);

            // Assert
            Assert.Throws<DomainException>(act);
        }

        [Fact]
        public void Create_Should_Throw_When_PerformedBy_Is_Empty()
        {
            // Arrange
            var assetId = Guid.NewGuid();
            var startedOn = DateTime.UtcNow;
            var description = "Description";
            var emptyPerformer = " ";

            // Act
            void act() => MaintenanceRecord.Create(assetId, startedOn, description, emptyPerformer);

            // Assert
            Assert.Throws<DomainException>(act);
        }
    }
}
