using CleanArchitecture.Cmms.Domain.Assets.ValueObjects;

namespace CleanArchitecture.Cmms.Domain.UnitTests.Assets.ValueObjects
{
    public class AssetLocationTests
    {
        [Fact]
        public void Create_Should_Assign_Properties()
        {
            // Arrange
            var site = "SiteA";
            var area = "Area1";
            var zone = "ZoneZ";

            // Act
            var location = AssetLocation.Create(site, area, zone);

            // Assert
            Assert.Equal(site, location.Site);
            Assert.Equal(area, location.Area);
            Assert.Equal(zone, location.Zone);
        }

        [Fact]
        public void ChangeArea_Should_Return_New_Instance_With_Updated_Area()
        {
            // Arrange
            var original = AssetLocation.Create("S", "A", "Z");
            var newArea = "B";

            // Act
            var updated = original.ChangeArea(newArea);

            // Assert
            Assert.Equal(newArea, updated.Area);
            Assert.Equal(original.Site, updated.Site);
            Assert.Equal(original.Zone, updated.Zone);
            Assert.NotSame(original, updated);
        }

        [Fact]
        public void ToString_Should_Compose_Site_Area_Zone()
        {
            // Arrange
            var location = AssetLocation.Create("S1", "A1", "Z1");

            // Act
            var text = location.ToString();

            // Assert
            Assert.Equal("S1-A1-Z1", text);
        }
    }
}
