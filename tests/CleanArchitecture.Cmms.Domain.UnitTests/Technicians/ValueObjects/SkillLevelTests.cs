using CleanArchitecture.Cmms.Domain.Technicians.ValueObjects;

namespace CleanArchitecture.Cmms.Domain.UnitTests.Technicians.ValueObjects
{
    public class SkillLevelTests
    {
        [Fact]
        public void Static_Properties_Should_Have_Expected_Ranks()
        {
            // Arrange
            SkillLevel apprentice = SkillLevel.Apprentice;
            SkillLevel journeyman = SkillLevel.Journeyman;
            SkillLevel master = SkillLevel.Master;

            // Act
            int apprenticeRank = apprentice.Rank;
            int journeymanRank = journeyman.Rank;
            int masterRank = master.Rank;

            // Assert
            Assert.Equal(1, apprenticeRank);
            Assert.Equal(2, journeymanRank);
            Assert.Equal(3, masterRank);
        }

        [Fact]
        public void IsHigherThan_Should_Return_Correct_Comparison()
        {
            // Arrange
            SkillLevel lower = SkillLevel.Apprentice;
            SkillLevel higher = SkillLevel.Master;

            // Act
            bool result = higher.IsHigherThan(lower);

            // Assert
            Assert.True(result);
        }

        [Fact]
        public void Equality_Should_Be_Value_Based()
        {
            // Arrange
            SkillLevel levelA = SkillLevel.Create("Custom", 5);
            SkillLevel levelB = SkillLevel.Create("Custom", 5);

            // Act
            bool areEqual = levelA == levelB;

            // Assert
            Assert.True(areEqual);
        }

        [Fact]
        public void ToString_Should_Return_LevelName()
        {
            // Arrange
            SkillLevel level = SkillLevel.Journeyman;

            // Act
            string text = level.ToString();

            // Assert
            Assert.Equal("Journeyman", text);
        }
    }
}
