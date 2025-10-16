using CleanArchitecture.Cmms.Domain.WorkOrders.ValueObjects;

namespace CleanArchitecture.Cmms.Domain.UnitTests.WorkOrders.ValueObjects
{
    public class LocationTests
    {
        [Fact]
        public void Create_Should_Set_Properties()
        {
            // Arrange
            string building = "B1";
            string floor = "F2";
            string room = "R3";

            // Act
            Location location = Location.Create(building, floor, room);

            // Assert
            Assert.Equal(building, location.Building);
            Assert.Equal(floor, location.Floor);
            Assert.Equal(room, location.Room);
        }

        [Fact]
        public void ChangeBuilding_Should_Return_New_With_Updated_Building()
        {
            // Arrange
            Location location = Location.Create("B1", "F2", "R3");

            // Act
            Location changed = location.ChangeBuilding("B2");

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
            Location location = Location.Create("B1", "F2", "R3");

            // Act
            Location changed = location.ChangeFloor("F3");

            // Assert
            Assert.Equal("B1", changed.Building);
            Assert.Equal("F3", changed.Floor);
            Assert.Equal("R3", changed.Room);
        }

        [Fact]
        public void ChangeRoom_Should_Return_New_With_Updated_Room()
        {
            // Arrange
            Location loc = Location.Create("B1", "F2", "R3");

            // Act
            Location changed = loc.ChangeRoom("R9");

            // Assert
            Assert.Equal("B1", changed.Building);
            Assert.Equal("F2", changed.Floor);
            Assert.Equal("R9", changed.Room);
        }

        [Fact]
        public void ToString_Should_Format_As_Building_Floor_Room()
        {
            // Arrange
            Location location = Location.Create("B", "F", "R");

            // Act
            string result = location.ToString();

            // Assert
            Assert.Equal("B:F:R", result);
        }

        [Fact]
        public void Equality_Should_Work_For_Same_Values()
        {
            // Arrange
            Location valueObjectOne = Location.Create("B1", "F2", "R3");
            Location valueObjectTwo = Location.Create("B1", "F2", "R3");

            // Act
            bool eq = valueObjectOne == valueObjectTwo;

            // Assert
            Assert.True(eq);
        }
    }
}
