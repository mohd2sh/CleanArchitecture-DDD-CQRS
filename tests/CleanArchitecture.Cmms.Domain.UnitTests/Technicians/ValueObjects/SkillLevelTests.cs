using CleanArchitecture.Cmms.Domain.Technicians.ValueObjects;

namespace CleanArchitecture.Cmms.Domain.UnitTests.Technicians.ValueObjects;

public class SkillLevelTests
{
    [Fact]
    public void Static_Properties_Should_Have_Expected_Ranks()
    {
        // Arrange
        var apprentice = SkillLevel.Apprentice;
        var journeyman = SkillLevel.Journeyman;
        var master = SkillLevel.Master;

        // Act
        var apprenticeRank = apprentice.Rank;
        var journeymanRank = journeyman.Rank;
        var masterRank = master.Rank;

        // Assert
        Assert.Equal(1, apprenticeRank);
        Assert.Equal(2, journeymanRank);
        Assert.Equal(3, masterRank);
    }

    [Fact]
    public void IsHigherThan_Should_Return_Correct_Comparison()
    {
        // Arrange
        var lower = SkillLevel.Apprentice;
        var higher = SkillLevel.Master;

        // Act
        var result = higher.IsHigherThan(lower);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void Equality_Should_Be_Value_Based()
    {
        // Arrange
        var levelA = SkillLevel.Create("Custom", 5);
        var levelB = SkillLevel.Create("Custom", 5);

        // Act
        var areEqual = levelA == levelB;

        // Assert
        Assert.True(areEqual);
    }

    [Fact]
    public void ToString_Should_Return_LevelName()
    {
        // Arrange
        var level = SkillLevel.Journeyman;

        // Act
        var text = level.ToString();

        // Assert
        Assert.Equal("Journeyman", text);
    }
}
