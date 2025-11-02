using CleanArchitecture.Cmms.Domain.Assets.ValueObjects;
using CleanArchitecture.Core.Domain.Abstractions;

namespace CleanArchitecture.Cmms.Domain.UnitTests.Assets.ValueObjects;

public class AssetTagTests
{
    [Fact]
    public void Create_Should_Trim_Value()
    {
        // Arrange
        var rawValue = "  TAG-1 ";

        // Act
        var tag = AssetTag.Create(rawValue);

        // Assert
        Assert.Equal("TAG-1", tag.Value);
    }

    [Fact]
    public void Create_Should_Throw_When_Empty()
    {
        // Arrange
        var empty = " ";

        // Act
        void act() => AssetTag.Create(empty);

        // Assert
        Assert.Throws<DomainException>(act);
    }

    [Fact]
    public void ToString_Should_Return_Value()
    {
        // Arrange
        var tag = AssetTag.Create("T-200");

        // Act
        var text = tag.ToString();

        // Assert
        Assert.Equal("T-200", text);
    }
}
