using CleanArchitecture.Cmms.Domain.WorkOrders.ValueObjects;

namespace CleanArchitecture.Cmms.Domain.UnitTests.WorkOrders.ValueObjects
{
    public class LocationTests
    {
        [Fact]
        public void Create_Should_Set_Properties()
        {
            // Arrange
            var building = "B1";
            var floor = "F2";
            var room = "R3";

            // Act
            var location = Location.Create(building, floor, room);

            // Assert
            Assert.Equal(building, location.Building);
            Assert.Equal(floor, location.Floor);
            Assert.Equal(room, location.Room);
        }

        [Fact]
        public void ChangeBuilding_Should_Return_New_With_Updated_Building()
        {
            // Arrange
            var location = Location.Create("B1", "F2", "R3");

            // Act
            var changed = location.ChangeBuilding("B2");

            // Assert
            Assert.Equal("B2", changed.Building);
            Assert.Equal("F2", changed.Floor);
            Assert.Equal("R3", changed.Room);
            Assert.NotSame(location, changed);
        }

        [Fact]
        public void ChangeFloor_Should_Return_New_With_Updated_Floor()
        {
            // Arrange
            var location = Location.Create("B1", "F2", "R3");

            // Act
            var changed = location.ChangeFloor("F3");

            // Assert
            Assert.Equal("B1", changed.Building);
            Assert.Equal("F3", changed.Floor);
            Assert.Equal("R3", changed.Room);
        }

        [Fact]
        public void ChangeRoom_Should_Return_New_With_Updated_Room()
        {
            // Arrange
            var loc = Location.Create("B1", "F2", "R3");

            // Act
            var changed = loc.ChangeRoom("R9");

            // Assert
            Assert.Equal("B1", changed.Building);
            Assert.Equal("F2", changed.Floor);
            Assert.Equal("R9", changed.Room);
        }

        [Fact]
        public void ToString_Should_Format_As_Building_Floor_Room()
        {
            // Arrange
            var location = Location.Create("B", "F", "R");

            // Act
            var result = location.ToString();

            // Assert
            Assert.Equal("B:F:R", result);
        }

        [Fact]
        public void Equality_Should_Work_For_Same_Values()
        {
            // Arrange
            var valueObjectOne = Location.Create("B1", "F2", "R3");
            var valueObjectTwo = Location.Create("B1", "F2", "R3");

            // Act
            var eq = valueObjectOne == valueObjectTwo;

            // Assert
            Assert.True(eq);
        }
    }
}
