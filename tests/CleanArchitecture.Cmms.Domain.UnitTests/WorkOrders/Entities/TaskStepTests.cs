using CleanArchitecture.Cmms.Domain.WorkOrders.Entities;

namespace CleanArchitecture.Cmms.Domain.UnitTests.WorkOrders.Entities;

public class TaskStepTests
{
    [Fact]
    public void Create_Should_Set_Description_And_Default_Completed_False()
    {
        // Arrange
        var desc = "Check filter";

        // Act
        var step = TaskStep.Create(desc);

        // Assert
        Assert.Equal(desc, step.Description);
        Assert.False(step.Completed);
    }

    [Fact]
    public void MarkCompleted_Should_Set_Completed_True()
    {
        // Arrange
        var step = TaskStep.Create("x");

        // Act
        step.MarkCompleted();

        // Assert
        Assert.True(step.Completed);
    }

    [Fact]
    public void MarkCompleted_Should_Be_Idempotent()
    {
        // Arrange
        var step = TaskStep.Create("x");
        step.MarkCompleted();

        // Act
        step.MarkCompleted();

        // Assert
        Assert.True(step.Completed);
    }
}
