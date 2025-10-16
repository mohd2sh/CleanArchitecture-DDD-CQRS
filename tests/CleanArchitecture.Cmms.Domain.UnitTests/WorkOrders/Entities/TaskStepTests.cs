using CleanArchitecture.Cmms.Domain.WorkOrders.Enitties;

namespace CleanArchitecture.Cmms.Domain.UnitTests.WorkOrders.Entities
{
    public class TaskStepTests
    {
        [Fact]
        public void Ctor_Should_Set_Description_And_Default_Completed_False()
        {
            // Arrange
            string desc = "Check filter";

            // Act
            TaskStep step = new TaskStep(desc);

            // Assert
            Assert.Equal(desc, step.Description);
            Assert.False(step.Completed);
        }

        [Fact]
        public void MarkCompleted_Should_Set_Completed_True()
        {
            // Arrange
            TaskStep step = new TaskStep("x");

            // Act
            step.MarkCompleted();

            // Assert
            Assert.True(step.Completed);
        }

        [Fact]
        public void MarkCompleted_Should_Be_Idempotent()
        {
            // Arrange
            TaskStep step = new TaskStep("x");
            step.MarkCompleted();

            // Act
            step.MarkCompleted();

            // Assert
            Assert.True(step.Completed);
        }
    }
}
