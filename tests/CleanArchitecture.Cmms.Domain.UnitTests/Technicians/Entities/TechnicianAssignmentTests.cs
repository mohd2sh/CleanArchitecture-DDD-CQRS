using CleanArchitecture.Cmms.Domain.Technicians.Entities;

namespace CleanArchitecture.Cmms.Domain.UnitTests.Technicians.Entities
{
    public class TechnicianAssignmentTests
    {
        [Fact]
        public void Create_Should_Set_Properties()
        {
            // Arrange
            Guid workOrderId = Guid.NewGuid();
            DateTime assignedOn = DateTime.UtcNow;

            // Act
            TechnicianAssignment assignment = TechnicianAssignment.Create(workOrderId, assignedOn);

            // Assert
            Assert.Equal(workOrderId, assignment.WorkOrderId);
            Assert.Equal(assignedOn, assignment.AssignedOn);
            Assert.False(assignment.IsCompleted);
            Assert.Null(assignment.CompletedOn);
        }

        [Fact]
        public void CompleteAssignment_Should_Set_CompletedOn_And_Be_Idempotent()
        {
            // Arrange
            Guid workOrderId = Guid.NewGuid();
            DateTime assignedOn = DateTime.UtcNow.AddMinutes(-5);
            TechnicianAssignment assignment = TechnicianAssignment.Create(workOrderId, assignedOn);
            DateTime completedOn = DateTime.UtcNow;

            // Act
            assignment.CompleteAssignment(completedOn);
            DateTime? firstCompletedOn = assignment.CompletedOn;
            assignment.CompleteAssignment(completedOn.AddMinutes(1)); // idempotent, should not change

            // Assert
            Assert.True(assignment.IsCompleted);
            Assert.Equal(completedOn, firstCompletedOn);
            Assert.Equal(firstCompletedOn, assignment.CompletedOn);
        }
    }
}
